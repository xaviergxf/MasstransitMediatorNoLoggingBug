using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Hosting;

namespace GettingStarted
{
    public class Worker : BackgroundService
    {
        readonly IMediator _mediator;

        public Worker(
            IMediator mediator//Remove this dependency to make looging work again
        )
        {
            _mediator = mediator;//Remove this dependency to make looging work again
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}