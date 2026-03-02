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
        public List<OrderLine> Lines { get; set; } = new List<OrderLine>();

        [Ignore]
        IEnumerable<IOrderLine> IOrder.Lines
        {
            get => Lines;
            set => Lines = value?.Cast<OrderLine>().ToList() ?? new List<OrderLine>();
        }

        public bool Equals(IOrder? other)
        {
            if (other == null) return false;

            return this.Id == other.Id;
        }
    }
}