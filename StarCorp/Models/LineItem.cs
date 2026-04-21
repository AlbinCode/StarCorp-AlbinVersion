using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class LineItem : IValidatableObject
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public uint Quantity { get; set; }
    public decimal Price { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var errors = new List<ValidationResult>();

        if (ProductId == Guid.Empty)
        {
            errors.Add(new ValidationResult("Product ID cannot be empty.", new[] { nameof(ProductId) }));
        }

        if (Quantity <= 0)
        {
            errors.Add(new ValidationResult("Quantity must be at least 1.", new[] { nameof(Quantity) }));
        }

        return errors;
    }
}