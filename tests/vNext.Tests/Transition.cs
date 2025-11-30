using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xunit;

namespace vNext.Tests
{
    public class Transition
    {
        [Fact]
        public async Task CheckTransitionState()
        {
            var status = new Status();
            var host = App.Create()
                .Workflow<Start>()
                .Workflow<Stop>()
                .Startup(new
                {
                    Services = new Action<IServiceCollection, IConfiguration, IHostEnvironment>((services, configuration, env) =>
                    {
                        services.AddSingleton(status);
                    }),
                    Configure = new Action<IConfiguration, ILogger<Transition>>((configuration, logs) =>
                    {
                    })
                })
                .Entry<Start>()
                .Build();

            await host.RunAsync(new CancellationTokenSource(1000).Token);

            Assert.True(status.Stop);
        }

        class Status
        {
            public bool Stop = false;
        }

        class Start : State
        {
            public Start()
            {
            }

            public override Result Activate(object? data)
            {
                return Navigate<Stop>();
            }
        }

        class Stop : State
        {
            readonly Status status;
            readonly App app;

            public Stop(Status status,
                App app)
            {
                this.status = status;
                this.app = app;
            }

            public override Result Activate(object? data)
            {
                status.Stop = true;

                app.Stop();

                return base.Activate(data);
            }
        }
    }
}