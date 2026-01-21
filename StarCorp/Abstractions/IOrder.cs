using System;
using System.Collections.Generic;

namespace StarCorp.Abstractions
{
    public interface IOrder
    {
        Guid Id { get; set; }
        string Buyer { get; set; }
        string DeliveryAddress { get; set; }
        decimal TotalValue { get; set; }
        IEnumerable<IOrderLine> Lines { get; set; }
    }

    public interface IOrderLine
    {
        Guid Id { get; set; }
        Guid OrderId { get; set; }
        Guid ProductId { get; set; }
        uint Quantity { get; set; }
    }
}