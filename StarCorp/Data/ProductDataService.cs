using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using StarCorp.Abstractions;
using StarCorp.Models;
using StarCorp.Exceptions;
namespace StarCorp.Data
{
    public interface IProductDataService
    {
        /// <summary>
        /// Saves the a new product to the data storage.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Is thrown if product already exists </exception>
        Task<IProduct> CreateProductAsync(IProduct product);

        /// <summary>
        ///  Returns queryable for all products in the date store.
        /// </summary>
        /// <returns></returns>
        Task<IQueryable<IProduct>> GetProductsAsync();

        /// <summary>
        /// Partial update of a single product record. Will update the specified product with any provided properties that are not null.
        /// </summary>
        /// <param name="product"></param>
        /// <returns>The modified product</returns>
        Task<IProduct> UpdateProductAsync(IProduct product);

        /// <summary>
        /// Deletes a product from the data store.
        /// </summary>
        /// <param name="product">The product to be deleted</param>
        Task DeleteProductAsync(IProduct product);
    }

    /// <summary>
    /// Simple product data service to read and update product information from csv
    /// </summary>
    public class ProductDataService : IProductDataService
    {
        private readonly AppDbContext _context;


        public ProductDataService(AppDbContext context)
        {

            _context = context;
        }

        private void ValidateProduct(IProduct product)
        {
            ArgumentNullException.ThrowIfNull(product);
            ArgumentNullException.ThrowIfNull(product.Id);
        }

        public async Task<IProduct> CreateProductAsync(IProduct product)
        {
            ValidateProduct(product);

            var exists = await _context.Products.AnyAsync(p => p.Id == product.Id);
            if (exists)
            {
                throw new ArgumentException("Cannot Create new product, Product with that ID already exists");
            }

            var newProduct = (Product)product;

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return newProduct;
        }

        public async Task<IQueryable<IProduct>> GetProductsAsync()
        {
            return await Task.FromResult<IQueryable<IProduct>>(_context.Products);
        }

        public async Task<IProduct> UpdateProductAsync(IProduct product)
        {
            ValidateProduct(product);

            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct == null)
            {
                throw new ResourceNotFoundException(nameof(Product), product.Id);
            }

            var newProduct = (Product)product;

            if (newProduct.Name != null)
            {
                existingProduct.Name = newProduct.Name;
            }

            if (newProduct.Description != null)
            {
                existingProduct.Description = newProduct.Description;
            }

            if (newProduct.Price > 0)
            {
                existingProduct.Price = newProduct.Price;
            }

            await _context.SaveChangesAsync();

            return existingProduct;
        }

        public async Task DeleteProductAsync(IProduct product)
        {
            ValidateProduct(product);

            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct != null)
            {
                _context.Products.Remove(existingProduct);
                await _context.SaveChangesAsync();
            }

        }
    }
}

