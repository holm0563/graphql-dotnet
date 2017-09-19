using System.Diagnostics;
using System.Threading;
using GraphQL.Conversion;
using GraphQL.Execution;
using GraphQL.Tests.StarWars;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;
using Xunit;
using Xunit.Abstractions;

namespace GraphQL.Tests.Execution.Performance
{
    public class StarWarsPerformanceTests : StarWarsTestBase
    {
        private readonly ITestOutputHelper _output;

        public StarWarsPerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Executes_StarWarsBasicQuery_Performant()
        {

            var query = @"
                query HeroNameAndFriendsQuery {
                  hero {
                    id
                    name
                    appearsIn
                    friends {
                      name
                      appearsIn
                    }
                  }
                }
            ";

            var smallListTimer = new Stopwatch();
            ExecutionResult runResult2 = null;
            smallListTimer.Start();

            for (int x = 0; x < 10000; x++)
            {
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
            }

            smallListTimer.Stop();

            _output.WriteLine($"Milliseconds: {smallListTimer.ElapsedMilliseconds}");

            Assert.Null(runResult2.Errors);
            Assert.True(smallListTimer.ElapsedMilliseconds < 1212 * 2); //machine specific data with a buffer
        }

        [Fact]
        public void Executes_StarWarsBasicQuery_Is_Performant()
        {

            var query = @"
                query HeroNameAndFriendsQuery {
                  hero {
                    id
                    name
                    appearsIn
                  }
                }
            ";

            var smallListTimer = new Stopwatch();
            ExecutionResult runResult2 = null;
            smallListTimer.Start();

            for (int x = 0; x < 100; x++)
            {
                //cache isn't doing any good here. 
                var executer = new DocumentExecuter(new GraphQLDocumentBuilder(), new CachedDocumentValidator(), new ComplexityAnalyzer());
                runResult2 = executer.ExecuteAsync(_ =>
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
            }

            smallListTimer.Stop();

            _output.WriteLine($"Time: {smallListTimer.Elapsed}");

            Assert.Null(runResult2.Errors);
            Assert.True(smallListTimer.ElapsedMilliseconds < 247 * 2); //machine specific data with a buffer
        }
    }
}
