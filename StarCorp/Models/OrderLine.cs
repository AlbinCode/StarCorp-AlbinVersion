using System;
using System.Collections.Generic;
using StarCorp.Abstractions;
namespace StarCorp.Models
{
    public class OrderLine : IOrderLine
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public uint Quantity { get; set; }
    }
}
