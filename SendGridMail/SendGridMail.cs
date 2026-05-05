using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;

namespace SendGridMail
{
    public class SendGridMailOrderConfirmation
    {
        private readonly ILogger _logger;

        public SendGridMailOrderConfirmation(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SendGridMailOrderConfirmation>();
        }

        [Function("SendOrderConfirmation")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            var orderData = await req.ReadFromJsonAsync<OrderConfirmationDetails>();

            if (orderData == null || string.IsNullOrEmpty(orderData.BuyerEmail))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Missing order info or BuyerEmail.");
                return badResponse;
            }

            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var client = new SendGridClient(apiKey);

            var senderEmail = Environment.GetEnvironmentVariable("SenderEmail");
            var from = new EmailAddress(senderEmail);

            var to = new EmailAddress(orderData.BuyerEmail, orderData.Buyer);

            var subject = $"Order Confirmation - {orderData.OrderId}";

            var plainTextContent =
                       $"Hi {orderData.Buyer}!\n\n" +
                       $"Thank you for your order.\n\n" +
                       $"Order ID: {orderData.OrderId}\n" +
                       $"Delivery Address: {orderData.DeliveryAddress}\n" +
                       $"Email: {orderData.BuyerEmail}\n" +
                       $"Total: {orderData.TotalValue} SEK";

            var htmlContent = 
                       $"Hi {orderData.Buyer}!<br><br>" +
                       $"Thank you for your order.<br><br>" +
                       $"Order ID: {orderData.OrderId}<br>" +
                       $"Delivery Address: {orderData.DeliveryAddress}<br>" +
                       $"Email: {orderData.BuyerEmail}<br>" +
                       $"Total: {orderData.TotalValue} SEK";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var sendGridResponse = await client.SendEmailAsync(msg);

            if (sendGridResponse.IsSuccessStatusCode)
            {
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteStringAsync("Email sent successfully.");
                return successResponse;
            }
            else
            {
                string sendGridErrorBody = await sendGridResponse.Body.ReadAsStringAsync();

                var errorResponse = req.CreateResponse(sendGridResponse.StatusCode);
                await errorResponse.WriteStringAsync($"Failed to send email through Sendgrid. {sendGridErrorBody}");
                return errorResponse;
            }
        }
    }

    public class OrderConfirmationDetails
    {
        public string Buyer { get; set; }
        public string BuyerEmail { get; set; }
        public Guid OrderId { get; set; }
        public string DeliveryAddress { get; set; }
        public decimal TotalValue { get; set; }
    }
}