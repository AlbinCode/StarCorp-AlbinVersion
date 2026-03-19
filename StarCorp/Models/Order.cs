using CsvHelper.Configuration.Attributes;
using StarCorp.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StarCorp.Models
{
    public class Order : IOrder
    {
        public Guid Id { get; set; }
        public string Buyer { get; set; }
        public string BuyerEmail { get; set; }
        public string DeliveryAddress { get; set; }
        public decimal TotalValue { get; set; }
        public List<LineItem> Lines { get; set; } = new List<LineItem>();

        [Ignore]
        IEnumerable<ILineItem> IOrder.Lines
        {
            get => Lines;
            set => Lines = value?.Cast<LineItem>().ToList() ?? new List<LineItem>();
        }

        public bool Equals(IOrder? other)
        {
            if (other == null) return false;

            return this.Id == other.Id;
        }
    }
}