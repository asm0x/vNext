using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Xunit;

namespace vNext.Tests
{
    public class Entry
    {
        [Fact]
        public async Task CheckEntryState()
        {
            var status = new Status();
            var host = App.Create()
                .Workflow<Start>()
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
                .Entry<Start>()
                .Build();

            await host.RunAsync(new CancellationTokenSource(1000).Token);

            Assert.True(status.Activated);
        }

        class Status
        {
            public bool Activated = false;
        }

        class Start : State
        {
            readonly Status status;
            readonly App app;

            public Start(Status status,
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

        [Fact]
        public void CheckDuplicatedEntryRegistration()
        {
            Assert.Throws<Exception>(() =>
            App.Create()
                .Entry<Start>()
                .Entry<Start>());
        }
    }
}