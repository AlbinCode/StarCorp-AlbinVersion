using System;
using StarCorp.Abstractions;

namespace StarCorp.Models
{
    public class Product : IProduct
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public uint Stock { get; set; }

        public bool Equals(IProduct other)
        {
            return Id == (other?.Id ?? Guid.Empty);
        }
    }
}
