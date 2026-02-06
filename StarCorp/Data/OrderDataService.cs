using CsvHelper;
using CsvHelper.Configuration;
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
        Task<Guid> CreateOrderAsync(IOrder order);
        Task<Guid> UpdateOrderAsync(IOrder order);
        Task<Guid> DeleteOrderAsync(Guid id);
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

        public async Task<Guid> UpdateOrderAsync(IOrder order)
        {
            var orders = new List<Order>();

            using (var reader = new StreamReader(ORDERS_FILE_PATH))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                orders = csv.GetRecordsAsync<Order>().ToBlockingEnumerable().ToList();
            }

            var existingOrderIndex = orders.FindIndex(o => o.Id == order.Id);

            if (existingOrderIndex == -1)
            {
                throw new ArgumentException("Ordern hittades ej.");
            }

            orders[existingOrderIndex] = (Order)order;

            using (var writer = new StreamWriter(ORDERS_FILE_PATH))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(orders);
            }

            if (File.Exists(ORDERLINES_FILE_PATH))
            {
                var allLines = new List<OrderLine>();

                using (var reader = new StreamReader(ORDERLINES_FILE_PATH))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    allLines = csv.GetRecordsAsync<OrderLine>().ToBlockingEnumerable().ToList();
                }

                allLines.RemoveAll(line => line.OrderId == order.Id);

                foreach (var line in order.Lines)
                {
                    var concreteLine = (OrderLine)line;

                    concreteLine.OrderId = order.Id;

                    if (concreteLine.Id == Guid.Empty) concreteLine.Id = Guid.NewGuid();

                    allLines.Add(concreteLine);
                }

                using (var writer = new StreamWriter(ORDERLINES_FILE_PATH))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(allLines);
                }
            }
            return order.Id;
        }
        public async Task<Guid> CreateOrderAsync(IOrder order)
        {
            var fileExists = File.Exists(ORDERS_FILE_PATH) && new FileInfo(ORDERS_FILE_PATH).Length > 0;
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = !fileExists
            };

            using (var writer = new StreamWriter(ORDERS_FILE_PATH, true))
            using (var csv = new CsvWriter(writer, config))
            {
                await csv.WriteRecordsAsync(new[] { order });
            }

            foreach (var line in order.Lines)
                line.OrderId = order.Id;

            var linesFileExists = File.Exists(ORDERLINES_FILE_PATH) && new FileInfo(ORDERLINES_FILE_PATH).Length > 0;
            var linesConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = !linesFileExists
            };
            using (var writer = new StreamWriter(ORDERLINES_FILE_PATH, true))
            using (var csv = new CsvWriter(writer, linesConfig))
            {
                await csv.WriteRecordsAsync(order.Lines);
            }

            return order.Id;
        }

        public async Task<Guid> DeleteOrderAsync(Guid id)
        {
            var orders = new List<Order>();
            using (var reader = new StreamReader(ORDERS_FILE_PATH))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                orders = csv.GetRecordsAsync<Order>().ToBlockingEnumerable().ToList();
            }

            var orderToDelete = orders.FirstOrDefault(o => o.Id == id);

            if (orderToDelete == null)
            {
                throw new ArgumentException("Ordern hittades ej.");
            }

            orders.Remove(orderToDelete);
            using (var writer = new StreamWriter(ORDERS_FILE_PATH))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(orders);
            }

            if (File.Exists(ORDERLINES_FILE_PATH))
            {
                var allLines = new List<OrderLine>();

                using (var reader = new StreamReader(ORDERLINES_FILE_PATH))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    allLines = csv.GetRecordsAsync<OrderLine>().ToBlockingEnumerable().ToList();
                }
                allLines.RemoveAll(line => line.OrderId == id);

                using (var writer = new StreamWriter(ORDERLINES_FILE_PATH))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(allLines);
                }
            }

            return id;
        }

        public async Task<Guid> SaveOrder(IOrder order)
        {
            bool fileExists = File.Exists(ORDERS_FILE_PATH) && new FileInfo(ORDERS_FILE_PATH).Length > 0;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = !fileExists
            };

            using (var writer = new StreamWriter(ORDERS_FILE_PATH, true))
            using (var csv = new CsvWriter(writer, config))
            {
                await csv.WriteRecordsAsync(new[] { order });
            }

            foreach (var line in order.Lines)
            {
                line.OrderId = order.Id;
            }

            bool linesFileExists = File.Exists(ORDERLINES_FILE_PATH) && new FileInfo(ORDERLINES_FILE_PATH).Length > 0;

            var linesConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = !linesFileExists
            };

            using (var writer = new StreamWriter(ORDERLINES_FILE_PATH, true))
            using (var csv = new CsvWriter(writer, linesConfig))
            {
                await csv.WriteRecordsAsync(order.Lines);
            }

            return order.Id;
        }
    }
}
