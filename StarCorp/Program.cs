using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StarCorp.Data;

namespace DevTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<IProductDataService, ProductDataService>();
            builder.Services.AddSingleton<IOrderDataService, OrderDataService>();

            // Set the working directory for the CSV files.
            var dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            if (dir != null)
                Directory.SetCurrentDirectory(dir.ToString());

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}