using Microsoft.Extensions.Logging;
using Quartz;
using StarCorp.Data;
using StarCorp.Logger;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StarCorp.Jobs
{
    public class SendEmailJob : IJob
    {
        private readonly IStarCorpLogger<SendEmailJob> _logger;

        public SendEmailJob(IStarCorpLogger<SendEmailJob> logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;

            var mailProperties = new
            {
                Buyer = dataMap.GetString("BuyerName"),
                BuyerEmail = dataMap.GetString("BuyerEmail"),
                DeliveryAddress = dataMap.GetString("DeliveryAddress"),
                OrderId = Guid.Parse(dataMap.GetString("OrderId")),
                TotalValue = decimal.Parse(dataMap.GetString("TotalValue"))
            };

            try
            {
                using var httpClient = new HttpClient();
                string functionUrl = "http://localhost:7071/api/SendOrderConfirmation";

                var response = await httpClient.PostAsJsonAsync(functionUrl, mailProperties);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully triggered email function for Order {OrderId} via Quartz", mailProperties.OrderId);
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Azure Function failed to send email. Status: {StatusCode}. Error: {ErrorContent}", response.StatusCode, errorContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Quartz failed to connect to the Azure Function for Order {OrderId}", mailProperties.OrderId);
            }
        }
    }
}