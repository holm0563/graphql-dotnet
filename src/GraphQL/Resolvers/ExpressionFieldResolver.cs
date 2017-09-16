using System;
using System.Linq.Expressions;
using GraphQL.Types;

namespace GraphQL.Resolvers
{
    public class ExpressionFieldResolver<TSourceType, TProperty> : IFieldResolver<TProperty>
    {
        private readonly Func<TSourceType, TProperty> _property;

        public ExpressionFieldResolver(Expression<Func<TSourceType, TProperty>> property)
        {
            _property = property.Compile();
        }

        public TProperty Resolve(ResolveFieldContext context)
        {
            return _property(context.As<TSourceType>().Source);
        }

        public bool RunThreaded()
        {
            return false;
        }

        object IFieldResolver.Resolve(ResolveFieldContext context)
        {
            return Resolve(context);
        }
    }
}
