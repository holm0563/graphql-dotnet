using System;
using System.Threading.Tasks;
using GraphQL.Types;

namespace GraphQL.Resolvers
{
    public class AsyncFuncFieldResolver<TReturnType> : FuncFieldResolver<TReturnType>
    {
        public AsyncFuncFieldResolver(Func<ResolveFieldContext, TReturnType> resolver):base(resolver)
        {
        }

        public override bool RunThreaded()
        {
            return true;
        }
    }

    public class AsyncFuncFieldResolver<TSourceType, TReturnType> : FuncFieldResolver<TSourceType, TReturnType>
    {
        public AsyncFuncFieldResolver(Func<ResolveFieldContext<TSourceType>, TReturnType> resolver):base(resolver)
        {
        }

        public override bool RunThreaded()
        {
            return true;
        }
    }
}
