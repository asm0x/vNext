using Drivers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtoBuf;
using Xunit;

namespace vNext.Tests
{
    public class IO
    {
        [Fact]
        public async Task DeviceIO()
        {
            var status = new Status();
            var host = App.Create()
                .Workflow<Start>()
                .Workflow<Stop>()
                .Device<Test>()
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
                .Entry<Start>()
                .Build();

            await host.RunAsync(new CancellationTokenSource(1000).Token);

            Assert.True(status.OK);
        }

        class Status
        {
            public bool OK = false;
        }

        class Test : Device
        {
            public override Type Control => typeof(Control);
            public override Type Events => typeof(Events);

            public override void Send(object data)
            {
                if (data is Control)
                {
                    Receive(new Events());
                }
            }
        }

        [ProtoContract]
        class Control
        {
        }

        [ProtoContract]
        class Events
        {
        }

        class Start : State
        {
            public override Result Activate(object? data)
            {
                Device(new Control());

                return base.Activate(data);
            }

            public override Result IO(object data)
            {
                if (data is Events)
                {
                    Navigate<Stop>();
                }

                return base.IO(data);
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
                status.OK = true;
                app.Stop();

                return base.Activate(data);
            }
        }
    }
}