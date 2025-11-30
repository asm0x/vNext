using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xunit;

namespace vNext.Tests
{
    public class States
    {
        [Fact]
        public async Task RegisterStatesFromAssembly()
        {
            var status = new Status();
            var host = App.Create()
                .Workflow(typeof(States).Assembly, type => type.GetInterfaces().Any(x => x == typeof(IStates)))
                .Startup(new
                {
                    Services = new Action<IServiceCollection, IConfiguration, IHostEnvironment>((services, configuration, env) =>
                    {
                        services.AddSingleton(status);
                    }),
                    Configure = new Action<IConfiguration, ILogger<States>>((configuration, logs) =>
                    {
                    })
                })
                .Entry<S1>()
                .Build();

            await host.RunAsync(new CancellationTokenSource(1000).Token);

            Assert.True(status.Activated);
        }

        interface IStates
        {
        }

        class Status
        {
            public bool Activated = false;
        }

        class S1 : State, IStates
        {
            public override Result Activate(object? data)
            {
                return Navigate<S2>();
            }
        }
        class S2 : State, IStates
        {
            public override Result Activate(object? data)
            {
                return Navigate<S3>();
            }
        }
        class S3 : State, IStates
        {
            readonly Status status;
            readonly App app;

            public S3(Status status,
                App app)
            {
                this.status = status;
                this.app = app;
            }

            public override Result Activate(object? data)
            {
                status.Activated = true;

                app.Stop();

                return base.Activate(data);
            }
        }
    }
}