using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StarCorp.Data;
using StarCorp.Endpoints;
using StarCorp.Logger;
using Quartz;
using System.IO;
using System.Reflection;

namespace StarCorp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("StarCorpInMemory");

                if (builder.Environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                }
            });

            builder.Services.AddAntiforgery();

            builder.Services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                // Add jobs and triggers here when needed.
            });

            builder.Services.Configure<QuartzOptions>(options =>
            {
                options["quartz.plugin.jobHistory.type"] =
                    "Quartz.Plugin.History.LoggingJobHistoryPlugin, Quartz.Plugins";

                options["quartz.plugin.triggerHistory.type"] =
                    "Quartz.Plugin.History.LoggingTriggerHistoryPlugin, Quartz.Plugins";
            });

            builder.Services.AddQuartzDashboard();

            builder.Services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            builder.Services.AddTransient(typeof(IStarCorpLogger<>), typeof(StarCorpLogger<>));
            builder.Services.AddScoped<IProductDataService, ProductDataService>();
            builder.Services.AddScoped<IOrderDataService, OrderDataService>();
            builder.Services.AddScoped<ICartService, CartDataService>();

            var dir = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            if (dir != null)
            {
                Directory.SetCurrentDirectory(dir.ToString());
            }

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseAntiforgery();

            app.MapProductEndpoints();
            app.MapOrderEndpoints();
            app.MapCartEndpoints();

            app.MapQuartzDashboard();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
            }

            app.Run();
        }
    }
}