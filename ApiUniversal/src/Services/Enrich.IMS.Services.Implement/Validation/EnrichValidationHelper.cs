using Enrich.Core.Container;
using Enrich.Dto;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Validation
{
    public partial class EnrichValidationHelper
    {
        private readonly IEnrichContainer _container;
        private readonly EnrichContext _context;

        public EnrichValidationHelper(IEnrichContainer container, EnrichContext context)
        {
            _container = container;
            _context = context;
        }

        public static List<Type> GetValidatorTypes()
        {
            var type = typeof(IValidator);

            var validatorTypes = typeof(EnrichValidationHelper).GetTypeInfo().Assembly
                .GetTypes()
                .Where(type.IsAssignableFrom)
                .Where(t => !t.GetTypeInfo().IsAbstract)
                .ToList();

            return validatorTypes;
        }

        #region ConfigFluentRule

        private static List<InternalConfigRule> ConfigRules = new List<InternalConfigRule>
        {
            new InternalConfigRule("required", new [] { EnrichValidationErrorCodes.Required, "NotNullValidator" }, "FIELD_REQUIRED"),
            new InternalConfigRule("email", new [] { EnrichValidationErrorCodes.Email }, "FIELD_EMAIL"),
            new InternalConfigRule("max", new [] { EnrichValidationErrorCodes.MaximumLength }, "FIELD_MAX"),
            new InternalConfigRule("min", new [] { EnrichValidationErrorCodes.MinimumLength }, "FIELD_MIN"),
            new InternalConfigRule("greater_zero", new [] { EnrichValidationErrorCodes.GreaterThanZero }, "FIELD_GREATER_ZERO")
        };

        private class InternalConfigRule
        {
            public string Rule { get; set; }

            public string[] ErrorCodes { get; set; }

            public string TransKey { get; set; }

            public InternalConfigRule(string rule, string[] errorCodes, string transKey)
            {
                Rule = rule;
                ErrorCodes = errorCodes;
                TransKey = transKey;
            }
        }

        #endregion
    }

    public sealed class EnrichValidationErrorCodes
    {
        public const string Required = "NotEmptyValidator";

        public const string MaximumLength = "MaximumLengthValidator";

        public const string MinimumLength = "MinimumLengthValidator";

        public const string Email = "EmailValidator";

        public const string GreaterThanZero = "GreaterThanZeroValidator";
    }
}
