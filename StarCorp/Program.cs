using CsvHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StarCorp.Carts;
using StarCorp.Data;
using StarCorp.Models;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace StarCorp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("StarCorpInMemory"));


            builder.Services.AddScoped<IProductDataService, ProductDataService>();
            builder.Services.AddScoped<IOrderDataService, OrderDataService>();
            builder.Services.AddScoped<ICartService, CartService>();
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
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.Database.EnsureCreated();
            }
            app.Run();
        }
    }
}