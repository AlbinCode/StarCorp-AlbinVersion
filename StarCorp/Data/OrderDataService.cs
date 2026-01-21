using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using StarCorp.Abstractions;

namespace StarCorp.Data
{
    public interface IOrderDataService
    {
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
