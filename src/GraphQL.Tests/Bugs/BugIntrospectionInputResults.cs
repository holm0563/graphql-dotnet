using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using GraphQL.Introspection;
using GraphQL.Types;
using Shouldly;
using Xunit;

namespace GraphQL.Tests.Bugs
{
    public class BugIntrospectionInputResults
    {
        private readonly IDocumentExecuter _executer = new DocumentExecuter();

        ExecutionResult ExecuteQuery(ISchema schema, string query)
        {
            return _executer.ExecuteAsync(schema, null, query, null, null, CancellationToken.None).Result;
        }

        [Fact]
        public void only_nullable_is_happy()
        {
            var schema = new IntrospectionInputSchema();
            
            var actual = ExecuteQuery(
                schema,
                @"{__type(name:""InputExample""){
                        name
                        kind
                        fields{
                            name
                            type{
                                kind
                                ofType{
                                    kind
                                }
                            }
                        }
                    }}"
            );

            var collection = actual.Data as IDictionary<string, object>;
            var mainObject = collection.First().Value as IDictionary<string, object>;
            var fields = mainObject["fields"];

            //should have 2 fields. see __Type.cs line 
            Assert.NotNull(fields);
        }

       
        public class IntrospectionInputSchema : Schema
        {
            public IntrospectionInputSchema()
            {
                var query = new ObjectGraphType();

                query.Field<StringGraphType>("testObject",
                    arguments: new QueryArguments(new QueryArgument<InputExample> {Name = "arg1"}),
                     resolve: c => "hi");

                Query = query;
            }
        }

        public class InputExample : InputObjectGraphType
        {
            public InputExample()
            {
                Field<StringGraphType>("profileImage", "profileImage of user.");
                Field<IntGraphType>("age", "user gender.");
            }
        }
    }

   
}
