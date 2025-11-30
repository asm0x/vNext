using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace vNext.Tests
{
    public class Startup
    {
        [Fact]
        public async Task WithAnonymousClass()
        {
            bool _services = false;
            bool _configure = false;

            var host = App.Create()
                .Startup(new
                {
                    Services = new Action<IServiceCollection, IConfiguration, IHostEnvironment>((services, configuration, env) =>
                    {
                        _services = true;
                    }),
                    Configure = new Action<IConfiguration, ILogger<Startup>>((configuration, logs) =>
                    {
                        _configure = true;
                    })
                })
                .Build();

            await host.RunAsync(new CancellationTokenSource(1000).Token);

            Assert.True(_services);
            Assert.True(_configure);
        }

        [Fact]
        public async Task WithStartupClass()
        {
            var host = App.Create()
                .Startup<Class>()
                .Build();

            await host.RunAsync(new CancellationTokenSource(1000).Token);

            Assert.True(Class._services);
            Assert.True(Class._configure);
        }

        class Class
        {
            public static bool _services = false;
            public static bool _configure = false;

            public void Services(IServiceCollection services,
                IConfiguration configuration,
                IHostEnvironment env)
            {
                _services = true;
            }

            public void Configure(IConfiguration configuration,
                ILogger<Class> logs)
            {
                _configure = true;
            }
        }

        [Fact]
        public void CheckDuplicatedStartupObjectRegistration()
        {
            Assert.Throws<Exception>(() =>
            App.Create()
                .Startup(new { })
                .Startup(new { }));
        }

        [Fact]
        public void CheckDuplicatedStartupClassRegistration()
        {
            Assert.Throws<Exception>(() =>
            App.Create()
                .Startup<Class>()
                .Startup<Class>());
        }
    }
}