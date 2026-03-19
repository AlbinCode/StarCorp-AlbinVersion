using System;
using System.Collections.Generic;
using StarCorp.Abstractions;
namespace StarCorp.Models
{
    public class LineItem : ILineItem
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public uint Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
