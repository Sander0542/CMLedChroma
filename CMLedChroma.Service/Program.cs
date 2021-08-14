using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sander0542.CMLedController;
using Sander0542.CMLedController.Abstractions;

namespace CMLedChroma.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) => {
                    services.AddSingleton<ILedControllerProvider, LedControllerProvider>();
                    services.AddHostedService<Worker>();
                });
    }
}
