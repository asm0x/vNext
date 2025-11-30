using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace vNext.Tests
{
    public class Overrides
    {
        [Fact]
        public async Task OverrideStates()
        {
            var status = new Status();
            var host = App.Create()
                .Workflow(typeof(Start).Assembly, type => type.GetInterfaces().Any(x => x == typeof(ITest)))
                .Workflow<Override>()
                .Startup(new
                {
                    Services = new Action<IServiceCollection, IConfiguration, IHostEnvironment>((services, configuration, env) =>
                    {
                        services.AddSingleton(status);
                    }),
                    Configure = new Action<IConfiguration, ILogger<Overrides>>((configuration, logs) =>
                    {
                    })
                })
                .Entry<Start>()
                .Build();

            await host.RunAsync(new CancellationTokenSource(1000).Token);

            Assert.True(status.OK);
        }

        interface ITest
        {
        }

        class Status
        {
            public bool OK = false;
        }

        class Start : State, ITest
        {
            public override Result Activate(object? data)
            {
                return Navigate<Check>();
            }
        }
        class Check : State, ITest
        {
            public override Result Activate(object? data)
            {
                return base.Activate(data);
            }
        }
        class Override : Check
        {
            readonly Status status;
            readonly App app;

            public Override(Status status,
                App app)
            {
                this.status = status;
                this.app = app;
            }

            public override Result Activate(object? data)
            {
                status.OK = true;

                app.Stop();

                return base.Activate(data);
            }
        }
    }
}