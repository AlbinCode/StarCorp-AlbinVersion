using CsvHelper;
using StarCorp.Abstractions;
using StarCorp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StarCorp.Data
{
    public interface IOrderDataService
    {
        Task<IQueryable<IOrder>> GetOrdersAsync();
        Task<Guid> SaveOrder(IOrder order);
    }

    /// <summary>
    /// Simple CSV data service to save orders.
    /// </summary>
    public class OrderDataService : IOrderDataService
    {
        private const string ORDERS_FILE_PATH = "Content/Orders.csv";
        private const string ORDERLINES_FILE_PATH = "Content/OrderLines.csv";

        public OrderDataService()
        {
            if (!File.Exists(ORDERS_FILE_PATH))
            {
                Directory.CreateDirectory("Content");
                using (File.Create(ORDERS_FILE_PATH)) { };
            }

            if (!File.Exists(ORDERLINES_FILE_PATH))
            {
                Directory.CreateDirectory("Content");
                using (File.Create(ORDERLINES_FILE_PATH)) { };
            }
        }

        public async Task<IQueryable<IOrder>> GetOrdersAsync()
        {
            var orders = new List<Order>();

            using (var reader = new StreamReader(ORDERS_FILE_PATH))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                orders = csv.GetRecordsAsync<Order>().ToBlockingEnumerable().ToList();
            }

            if (File.Exists(ORDERLINES_FILE_PATH))
            {
                var allOrderLines = new List<OrderLine>();

                using (var reader = new StreamReader(ORDERLINES_FILE_PATH))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    allOrderLines = csv.GetRecordsAsync<OrderLine>().ToBlockingEnumerable().ToList();
                }

                foreach (var order in orders)
                {
                    var myLines = allOrderLines.Where(line => line.OrderId == order.Id).ToList();

                    order.Lines = myLines;
                }
            }
            return orders.AsQueryable();
        }



        public async Task<Guid> SaveOrder(IOrder order)
        {
            using (var writer = new StreamWriter(ORDERS_FILE_PATH, true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(new[] { order });
            }

            foreach (var line in order.Lines)
                line.OrderId = order.Id;

            using (var writer = new StreamWriter(ORDERLINES_FILE_PATH, true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(order.Lines);
            }

            return order.Id;
        }
    }
}
