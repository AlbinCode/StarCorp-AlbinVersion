using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace StarCorp.Exceptions
{
    public class ModelValidationException : ValidationException
    {
        public List<string> ValidationErrors { get; }

        public ModelValidationException(string modelName, List<ValidationResult> validationResults)
        {
            ValidationErrors = validationResults
                .Select(r => r.ErrorMessage ?? "Unknown error")
                .ToList();
        }

        public override string Message
        {
            get
            {
                return "Validation failed: " + string.Join(" , ", ValidationErrors);
            }
        }

        public static void ThrowIfInvalid(object model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var context = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);

            if (!isValid)
            {
                throw new ModelValidationException(model.GetType().Name, validationResults);
            }
        }
    }
}

