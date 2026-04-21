using CsvHelper.Configuration.Attributes;
using StarCorp.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace StarCorp.Models
{
    public class Order : IOrder, IValidatableObject
    {
        public Guid Id { get; set; }
        public Guid BuyerId { get; set; }
        public Buyer Buyer { get; set; }
        public decimal TotalValue { get; set; }
        public List<LineItem> Lines { get; set; } = new List<LineItem>();

        [Ignore]
        IEnumerable<ILineItem> IOrder.Lines
        {
            get => Lines.Cast<ILineItem>();
            set => Lines = value?.Cast<LineItem>().ToList() ?? new List<LineItem>();
        }

        public bool Equals(IOrder? other)
        {
            if (other == null) return false;

            return this.Id == other.Id;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();

            if (Buyer == null)
            {
                errors.Add(new ValidationResult("An order must have a buyer attached to it.", new[] { nameof(Buyer) }));
            }

            if (Lines == null || !Lines.Any())
            {
                errors.Add(new ValidationResult("An order must contain at least one item.", new[] { nameof(Lines) }));
            }

            return errors;
        }
    }
}