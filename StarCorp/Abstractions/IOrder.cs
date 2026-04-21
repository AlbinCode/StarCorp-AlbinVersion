using StarCorp.Models;
using System;
using System.Collections.Generic;

namespace StarCorp.Abstractions
{
    public interface IOrder : IEquatable<IOrder>
    {
        Guid Id { get; set; }
        public Guid BuyerId { get; set; }
        Buyer Buyer { get; set; }
        decimal TotalValue { get; set; }
        IEnumerable<ILineItem> Lines { get; set; }
    }

    public interface ILineItem
    {
        Guid Id { get; set; }
        Guid ProductId { get; set; }
        uint Quantity { get; set; }
        decimal Price { get; set; }
    }
}