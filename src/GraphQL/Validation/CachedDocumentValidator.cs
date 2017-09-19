using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace GraphQL.Validation
{
    public class CachedDocumentValidator : IDocumentValidator
    {
        public CachedDocumentValidator(IDocumentValidator baseValidator)
        {
            BaseValidator = baseValidator ?? throw new ArgumentNullException(nameof(baseValidator));
        }

        public CachedDocumentValidator()
        {
            BaseValidator = new DocumentValidator();
        }

        private IDocumentValidator BaseValidator { get; }

        private ConcurrentDictionary<string, IValidationResult> Cache { get; } =
            new ConcurrentDictionary<string, IValidationResult>();

        public IValidationResult Validate(
            string originalQuery,
            ISchema schema,
            Document document,
            IEnumerable<IValidationRule> rules = null,
            object userContext = null)
        {
            return Cache.GetOrAdd(originalQuery,
                t => BaseValidator.Validate(originalQuery, schema, document, rules, userContext));
        }
    }
}
