using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CsvHelper;
using StarCorp.Abstractions;
using StarCorp.Models;

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
        private const string PRODUCTS_FILE_PATH = "Content/Products.csv";

        public ProductDataService()
        {
            if (!File.Exists(PRODUCTS_FILE_PATH))
            {
                Directory.CreateDirectory("Content");
                using (File.Create(PRODUCTS_FILE_PATH)) { }
                ;
            }
        }

        private async Task WriteProductsAsync(IEnumerable<IProduct> products)
        {
            using (var writer = new StreamWriter(PRODUCTS_FILE_PATH))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(products);
            }
            return;
        }

        private void ValidateProduct(IProduct product)
        {
            ArgumentNullException.ThrowIfNull(product);
            ArgumentNullException.ThrowIfNull(product.Id);
        }

        public async Task<IProduct> CreateProductAsync(IProduct product)
        {
            ValidateProduct(product);

            var products = (await GetProductsAsync()).ToList();
            if (products.FirstOrDefault(p => p.Id == product.Id) != null)
                throw new ArgumentException("Cannot create new product. Product with this ID already exist");

            products.Add(product);
            await WriteProductsAsync(products);
            return product;
        }

        public async Task<IQueryable<IProduct>> GetProductsAsync()
        {
            using (var reader = new StreamReader(PRODUCTS_FILE_PATH))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var products = csv.GetRecordsAsync<Product>().ToBlockingEnumerable().ToList();
                return products.AsQueryable();
            }
        }

        public async Task<IProduct> UpdateProductAsync(IProduct product)
        {
            ValidateProduct(product);

            var products = (await GetProductsAsync()).ToList();
            var updatingProductIndex = products.IndexOf(product);

            if (updatingProductIndex < 0 || updatingProductIndex >= products.Count)
                throw new ArgumentException("Cannot update product. Product doesn't exist. Ensure Product.Id is correct.");

            var updatingProduct = products[updatingProductIndex];

            var properties = typeof(Product).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in properties)
            {
                var incomingValue = prop.GetValue(product);
                if (incomingValue != null)
                    prop.SetValue(updatingProduct, incomingValue);
            }
            await WriteProductsAsync(products);

            return updatingProduct;
        }

        public async Task DeleteProductAsync(IProduct product)
        {
            ValidateProduct(product);
            var products = (await GetProductsAsync()).ToList();
            products.Remove(product);
            await WriteProductsAsync(products);
        }
    }
}
