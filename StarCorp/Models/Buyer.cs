using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StarCorp.Models
{
    public class Buyer : IValidatableObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string DeliveryAddress { get; set; }
        public int PostalCode { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();
          
            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add(new ValidationResult("Buyer has to have a name.", new[] { nameof(Buyer.Name) }));
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                errors.Add(new ValidationResult("Buyer has to have an email.", new[] { nameof(Buyer.Email) }));
            }

            if (string.IsNullOrWhiteSpace(DeliveryAddress))
            {
                errors.Add(new ValidationResult("Buyer has to have a delivery address.", new[] { nameof(Buyer.DeliveryAddress) }));
            }


            return errors;
        }
    }
}
