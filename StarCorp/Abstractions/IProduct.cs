using System;

namespace StarCorp.Abstractions
{
    public interface IProduct : IEquatable<IProduct>
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string Brand { get; set; }
        string Category { get; set; }
        decimal Price { get; set; }
        uint Stock { get; set; }
    }
}