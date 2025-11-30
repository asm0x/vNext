using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xunit;

namespace vNext.Tests
{
    public class Lifecycle
    {
        [Fact]
        public async Task CheckLifecycleState()
        {
            var status = new Status();
            var host = App.Create()
                .Workflow<S1>()
                .Workflow<S2>()
                .Startup(new
                {
                    Services = new Action<IServiceCollection, IConfiguration, IHostEnvironment>((services, configuration, env) =>
                    {
                        services.AddSingleton(status);
                    }),
                    Configure = new Action<IConfiguration, ILogger<Lifecycle>>((configuration, logs) =>
                    {
                    })
                })
                .Entry<S1>()
                .Build();

            await host.RunAsync(new CancellationTokenSource(1000).Token);

            Assert.Equal("A1D1A2", status.Events);
        }

        class Status
        {
            public string Events = "";
        }

        class S1 : State
        {
            readonly Status status;

            public S1(Status status)
            {
                this.status = status;
            }

            public override Result Activate(object? data)
            {
                status.Events += "A1";

                return Navigate<S2>();
            }

            public override void Dispose()
            {
                status.Events += "D1";

                base.Dispose();
            }
        }

        class S2 : State
        {
            readonly Status status;
            readonly App app;

            public S2(Status status,
                App app)
            {
                this.status = status;
                this.app = app;
            }

            public override Result Activate(object? data)
            {
                status.Events += "A2";

                app.Stop();

                return base.Activate(data);
            }
        }
    }
}