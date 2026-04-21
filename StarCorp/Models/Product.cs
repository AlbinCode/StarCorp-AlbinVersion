using System;
using StarCorp.Abstractions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StarCorp.Models
{
    public class Product : IProduct, IValidatableObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public uint Stock { get; set; }

        public bool Equals(IProduct other)
        {
            return Id == (other?.Id ?? Guid.Empty);
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();

            if (Id == Guid.Empty)
            {
                errors.Add(new ValidationResult("ID cannot be empty.", new[] { nameof(Id) }));
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add(new ValidationResult("Product has to have a name.", new[] { nameof(Name) }));
            }

            if (Price <= 0)
            {
                errors.Add(new ValidationResult("Price has to be more than 0.", new[] { nameof(Price) }));
            }

            if (string.IsNullOrWhiteSpace(Brand))
            {
                errors.Add(new ValidationResult("It must have a brand connnected to product.", new[] { nameof(Brand) }));
            }

            return errors;
        }
    }
}
