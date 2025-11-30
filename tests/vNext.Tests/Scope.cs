using Drivers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace vNext.Tests
{
    public class Scope
    {
        [Fact]
        public async Task CheckStatesScope()
        {
            var status = new Status();
            var host = App.Create()
                .Device<Pulse>()
                .Workflow<S0>()
                .Workflow<S1>()
                .Workflow<S2>()
                .Workflow<S3>()
                .Startup(new
                {
                    Services = new Action<IServiceCollection, IConfiguration, IHostEnvironment>((services, configuration, env) =>
                    {
                        services.AddSingleton(status);

                        services.AddTransient<Transient>();
                        services.AddScoped<Scoped>();
                    }),
                    Configure = new Action<IConfiguration, ILogger<Lifecycle>>((configuration, logs) =>
                    {
                    })
                })
                .Entry<S0>()
                .Build();

            await host.RunAsync(new CancellationTokenSource(1000).Token);

            Assert.Equal("11230303", status.Data);
        }

        class Status
        {
            public string Data = "";
        }

        class Scoped
        {
            public int N { get; set; } = 0;
        }

        class Transient
        {
            public int N { get; set; } = 0;
        }

        class Pulse : Device
        {
            public override Type Control => typeof(Control);
            public override Type Events => typeof(Events);


            public Pulse(App app)
            {
                var cancellation = new CancellationTokenSource();

                Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(100);

                        Send(new Control());
                    }
                },
                cancellation.Token);

                app.Exit += () => cancellation.Cancel();
            }

            public override void Send(object data)
            {
                if (data is Control)
                    Receive(new Events());
            }
        }

        class Control
        {
        }

        class Events
        {
        }

        class S0 : State
        {
            readonly Status status;
            readonly Transient transient;
            readonly Scoped scoped;

            public S0(Status status,
                Transient transient,
                Scoped scoped)
            {
                this.status = status;
                this.transient = transient;
                this.scoped = scoped;

                transient.N++;
                scoped.N++;
            }

            public override Result Activate(object? data)
            {
                status.Data += transient.N;
                status.Data += scoped.N;

                Work(() => Navigate<S1>());

                return base.Activate(data);
            }
        }

        class S1 : State
        {
            readonly Status status;
            readonly Transient transient;
            readonly Scoped scoped;

            public S1(Status status,
                Transient transient,
                Scoped scoped)
            {
                this.status = status;
                this.transient = transient;
                this.scoped = scoped;

                transient.N += 2;
                scoped.N += 2;
            }

            public override Result Activate(object? data)
            {
                status.Data += transient.N;
                status.Data += scoped.N;

                return Navigate<S2>();
            }
        }

        class S2 : State
        {
            readonly Status status;
            readonly Transient transient;
            readonly Scoped scoped;

            public S2(Status status,
                Transient transient,
                Scoped scoped)
            {
                this.status = status;
                this.transient = transient;
                this.scoped = scoped;
            }

            public override Result Activate(object? data)
            {
                status.Data += transient.N;
                status.Data += scoped.N;

                return base.Activate(data);
            }

            public override Result IO(object data)
            {
                if (data is Events)
                {
                    Navigate<S3>();
                }

                return base.IO(data);
            }
        }

        class S3 : State
        {
            readonly Status status;
            readonly Transient transient;
            readonly Scoped scoped;
            readonly App app;

            public S3(Status status,
                Transient transient,
                Scoped scoped,
                App app)
            {
                this.status = status;
                this.transient = transient;
                this.scoped = scoped;
                this.app = app;
            }

            public override Result Activate(object? data)
            {
                status.Data += transient.N;
                status.Data += scoped.N;

                app.Stop();

                return base.Activate(data);
            }
        }
    }
}