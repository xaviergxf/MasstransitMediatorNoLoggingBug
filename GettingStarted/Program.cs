
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Microsoft.Extensions.Configuration;

namespace GettingStarted
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hbc, lc) =>
                {
                    lc.ReadFrom.Configuration(hbc.Configuration)
                      .Enrich.FromLogContext()
                      .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                        .WithDefaultDestructurers()
                        .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() })
                       )
                      .Enrich.WithMachineName();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMediator(x =>
                    {
                        x.AddConsumers(typeof(Program).Assembly);
                    });
                    services.AddMassTransit(x =>
                    {
                        x.AddRider(rider =>
                        {
                            rider.AddConsumer<MessageConsumer>();
                            rider.UsingKafka((context, kafka) =>
                            {
                                kafka.Host(new string[] { "kfk01-dev-gva:9092" }, host =>
                                {
                                });
                                kafka.SecurityProtocol = Confluent.Kafka.SecurityProtocol.Plaintext;
                                kafka.TopicEndpoint<Message>("sample", "facade-abc", kre =>
                                {
                                    kre.ConfigureConsumer<MessageConsumer>(context);
                                });
                            });
                        });
                        x.UsingInMemory((context, cfg) =>
                        {
                            cfg.ConfigureEndpoints(context);
                        });                        
                    });
                    services.AddMassTransitHostedService();

                    services.AddHostedService<Worker>();
                });
    }
}
