using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using GraphQL.Conversion;
using GraphQL.Types;
using Xunit;

namespace GraphQL.Tests.Execution.Performance
{
    public class ListPerformanceTests : QueryTestBase<ListPerformanceSchema>
    {
        public ListPerformanceTests()
        {
            Services.Register<PeopleType>();

            Services.Singleton(() => new ListPerformanceSchema(type => (GraphType)Services.Get(type)));

            _people = new List<Person>();

            var garfield = new Cat
            {
                Name = "Garfield",
                Meows = false
            };

            var odie = new Dog
            {
                Name = "Odie",
                Barks = true
            };

            var liz = new Person
            {
                Name = "Liz",
                Pets = new List<IPet>(),
                Friends = new List<INamed>()
            };

            for (var x = 0; x < PerformanceIterations; x++)
            {
                var person = new Person
                {
                    Name = $"Person {x}",
                    Pets = new List<IPet>
                    {
                        garfield,
                        odie
                    },
                    Friends = new List<INamed>
                    {
                        liz,
                        odie
                    }
                };

                _people.Add(person);
            }
        }

        private const int PerformanceIterations = 100000;
        private readonly List<Person> _people;

        private dynamic PeopleList => new
        {
            people = _people
        };

        private dynamic PeopleListSmall => new
        {
            people = _people.Take(PerformanceIterations / 10)
        };

        [Fact]
        public void Executes_Lists_Are_Performant()
        {
            var query = @"
                query AQuery {
                    people{name
                    pets { ... on Named{name}}
                    friends { ... on Named{name}}}
                }
            ";

            query = @"
                query AQuery {
                    people{name
                   }
                }
            ";

            //let everything initialize
            var runResultPreflight = Executer.ExecuteAsync(_ =>
            {
                _.Schema = Schema;
                _.Query = query;
                _.Root = PeopleListSmall;
                _.Inputs = null;
                _.UserContext = null;
                _.CancellationToken = default(CancellationToken);
                _.ValidationRules = null;
                _.FieldNameConverter = new CamelCaseFieldNameConverter();
            }).GetAwaiter().GetResult();

            var smallListTimer = new Stopwatch();

            smallListTimer.Start();

            var runResult2 = Executer.ExecuteAsync(_ =>
            {
                _.Schema = Schema;
                _.Query = query;
                _.Root = PeopleListSmall;
                _.Inputs = null;
                _.UserContext = null;
                _.CancellationToken = default(CancellationToken);
                _.ValidationRules = null;
                _.FieldNameConverter = new CamelCaseFieldNameConverter();
            }).GetAwaiter().GetResult();

            smallListTimer.Stop();

            var largeListTimer = new Stopwatch();

            largeListTimer.Start();

            var runResult = Executer.ExecuteAsync(_ =>
            {
                _.Schema = Schema;
                _.Query = query;
                _.Root = PeopleList;
                _.Inputs = null;
                _.UserContext = null;
                _.CancellationToken = default(CancellationToken);
                _.ValidationRules = null;
                _.FieldNameConverter = new CamelCaseFieldNameConverter();
            }).GetAwaiter().GetResult();

            largeListTimer.Stop();

            var differential = largeListTimer.ElapsedMilliseconds - smallListTimer.ElapsedMilliseconds;

            Assert.Null(runResult.Errors);
            Assert.Null(runResult2.Errors);

            //Before performance improvements change largeListTimer.ElapsedMilliseconds = 1510, smallListTimer = 147, with changes 545, 61
            //Test in a machine agnostic manner, we want better than O(N) performance
            Assert.True(differential < smallListTimer.ElapsedMilliseconds * 4);
        }
    }

    public class PeopleType : ObjectGraphType
    {
        public PeopleType()
        {
            Name = "People";

            Field<ListGraphType<PersonType>>("people");
        }
    }

    public class ListPerformanceSchema : Schema
    {
        public ListPerformanceSchema(Func<Type, GraphType> resolveType)
            : base(resolveType)
        {
            Query = (PeopleType)resolveType(typeof(PeopleType));
        }
    }
}
