using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using StarCorp.Abstractions;
namespace StarCorp.Models
{
    public class LineItem : ILineItem
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public uint Quantity { get; set; }
        public bool InStock { get; set; } = false;
        public decimal Price { get; set; }

    
    }
}
