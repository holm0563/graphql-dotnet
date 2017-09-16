using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Conversion;
using GraphQL.Execution;
using GraphQL.Types;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;
using Xunit;

namespace GraphQL.Tests.Execution.Performance
{
    public class ThreadPerformanceTests : QueryTestBase<ThreadPerformanceTests.ThreadPerformanceSchema>
    {
        public ThreadPerformanceTests()
        {
            Services.Register<PerfQuery>();

            Services.Singleton(() => new ThreadPerformanceSchema(type => (GraphType)Services.Get(type)));
        }

        public class PerfQuery : ObjectGraphType<object>
        {
            public PerfQuery()
            {
                Name = "Query";

                //todo: finish and make a resolver type that defaults to threads
                FieldAsync<StringGraphType>("halfSecond", resolve: async c => Get(500, "Half"));
                FieldAsync<StringGraphType>("quarterSecond", resolve: async c=>Get(500, "Quarter"));
            }

            private string Get(int milliseconds, string result)
            {
                Thread.Sleep(milliseconds);

                return result;
            }
        }

        public class ThreadPerformanceSchema : Schema
        {
            public ThreadPerformanceSchema(Func<Type, GraphType> resolveType)
                : base(resolveType)
            {
                Query = (PerfQuery)resolveType(typeof(PerfQuery));
            }
        }

        [Fact]
        public void Executes_IsQuickerThanTotalTaskTime()
        {

            var query = @"
                query HeroNameAndFriendsQuery {
                  halfSecond,
                  quarterSecond
                }
            ";

            var smallListTimer = new Stopwatch();
            ExecutionResult runResult2 = null;
            smallListTimer.Start();

            runResult2 = Executer.ExecuteAsync(_ =>
            {
                _.Schema = Schema;
                _.Query = query;
                _.Root = null;
                _.Inputs = null;
                _.UserContext = null;
                _.CancellationToken = default(CancellationToken);
                _.ValidationRules = null;
                _.FieldNameConverter = new CamelCaseFieldNameConverter();
            }).GetAwaiter().GetResult();

            smallListTimer.Stop();

            Assert.Null(runResult2.Errors);

            Assert.True(smallListTimer.ElapsedMilliseconds < 800);
        }
    }
}
