using System.Diagnostics;
using System.Threading;
using GraphQL.Conversion;
using GraphQL.Execution;
using GraphQL.Tests.StarWars;
using GraphQL.Validation;
using GraphQL.Validation.Complexity;
using Xunit;

namespace GraphQL.Tests.Execution.Performance
{
    public class StarWarsPerformanceTests : StarWarsTestBase
    {

        [Fact]
        public void Executes_StarWarsBasicQuery_Performant()
        {

            var query = @"
                query HeroNameAndFriendsQuery {
                  hero {
                    id
                    name
                    friends {
                      name
                    }
                  }
                }
            ";

            var smallListTimer = new Stopwatch();
            ExecutionResult runResult2 = null;
            smallListTimer.Start();

            for (int x = 0; x < 100; x++)
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

            Assert.Null(runResult2.Errors);

            Assert.True(smallListTimer.ElapsedMilliseconds < 305 * 2); //machine specific data with a buffer
        }

        [Fact]
        public void Executes_StarWarsBasicQuery_Is_Performant()
        {
           
            var query = @"
                query HeroNameAndFriendsQuery {
                  hero {
                    id
                    name
                    friends {
                      name
                    }
                  }
                }
            ";

            var smallListTimer = new Stopwatch();
            ExecutionResult runResult2 = null;
            smallListTimer.Start();

            for (int x = 0; x < 100; x++)
            {
                var executer = new DocumentExecuter(new GraphQLDocumentBuilder(), new DocumentValidator(), new ComplexityAnalyzer());
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

            Assert.Null(runResult2.Errors);

            Assert.True(smallListTimer.ElapsedMilliseconds < 327 * 2); //machine specific data with a buffer
        }
    }
}
