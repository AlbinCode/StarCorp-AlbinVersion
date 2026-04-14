using StarCorp.Models;
using System;
using System.Collections.Generic;

namespace StarCorp.Abstractions
{
    public interface IOrder : IEquatable<IOrder>
    {
        Guid Id { get; set; }
        string Buyer { get; set; }
        string BuyerEmail { get; set; }
        string DeliveryAddress { get; set; }
        decimal TotalValue { get; set; }
        IEnumerable<ILineItem> Lines { get; set; }
    }

    public interface ILineItem
    {
        Guid Id { get; set; }
        Guid ProductId { get; set; }
        public bool InStock { get; set; }
        uint Quantity { get; set; }
        decimal Price { get; set; }
    }
}