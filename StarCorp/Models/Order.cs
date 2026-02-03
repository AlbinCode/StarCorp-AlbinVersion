using System;
using System.Collections.Generic;
using StarCorp.Abstractions;

namespace StarCorp.Models
{
    public class Order : IOrder
    {
        public Guid Id { get; set; }
        public string Buyer { get; set; }
        public string DeliveryAddress { get; set; }
        public decimal TotalValue { get; set; }
        public IEnumerable<IOrderLine> Lines { get; set; }

        public bool Equals(IOrder? other)
        {
            if (other == null) return false;

            return this.Id == other.Id;
        }
    }
}
    
