using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace vNext.Tests
{
    public class Failure
    {
        [Fact]
        public async Task CheckFailureState()
        {
            var status = new Status();
            var host = App.Create()
                .Workflow<Start>()
                .Workflow<Crash>()
                .Startup(new
                {
                    Services = new Action<IServiceCollection, IConfiguration, IHostEnvironment>((services, configuration, env) =>
                    {
                        services.AddSingleton(status);
                    }),
                    Configure = new Action<IConfiguration, ILogger<Entry>>((configuration, logs) =>
                    {
                    })
                })
                .Failure<Crash>()
                .Entry<Start>()
                .Build();

            await host.RunAsync(new CancellationTokenSource(1000).Token);

            Assert.True(status.Failure);
        }

        class Status
        {
            public bool Failure = false;
        }

        class Start : State
        {
            public override Result Activate(object? data)
            {
                throw new Exception();
            }
        }

        class Crash : State
        {
            readonly Status status;
            readonly App app;

            public Crash(Status status,
                App app)
            {
                this.status = status;
                this.app = app;
            }

            public override Result Activate(object? data)
            {
                status.Failure = true;

                app.Stop();

                return base.Activate(data);
            }
        }
    }
}