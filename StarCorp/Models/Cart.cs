using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace StarCorp.Models
{
    public class Cart : IValidatableObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? BuyerId { get; set; }
        public List<LineItem> LineItems { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount => LineItems.Sum(i => i.Price * (decimal)i.Quantity);
        public int TotalItems => (int)LineItems.Sum(i => i.Quantity);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();

            if (Id == Guid.Empty)
            {
                errors.Add(new ValidationResult("Cart ID cannot be empty.", new[] { nameof(Id) }));
            }

            return errors;
        }
    }
}