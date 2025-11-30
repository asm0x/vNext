using System.Reflection;

namespace vNext
{
    public interface IStartup
    {
        object Value { get; }
    }

    partial class App
    {
        partial class Definition
        {
            bool _startup = false;

            public IDefinition Startup<T>()
                where T : class
            {
                if (_startup)
                    throw new Exception("Startup can be used only once");

                host.ConfigureServices((context, services) =>
                {
                    var startup = Activator.CreateInstance<T>();
                    services.AddSingleton<IStartup>(new Start(startup));

                    Services(startup, context, services);
                });

                _startup = true;

                return this;
            }

            public IDefinition Startup(object startup)
            {
                if (_startup)
                    throw new Exception("Startup can be used only once");

                host.ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IStartup>(new Start(startup));

                    Services(startup, context, services);
                });

                _startup = true;

                return this;
            }

            static void Services(object startup,
                HostBuilderContext context, IServiceCollection services)
            {
                var signature = new Type[]
                {
                    typeof(IServiceCollection),
                    typeof(IConfiguration),
                    typeof(IHostEnvironment)
                };

                var args = new object[]
                {
                    services,
                    context.Configuration,
                    context.HostingEnvironment
                };

                services.AddSingleton(startup);

                var function = startup.GetType().GetMethod("Services", signature);
                if (function is not null)
                {
                    function.Invoke(startup, args);
                    return;
                }

                var property = startup.GetType().GetProperty("Services")?.GetValue(startup) as Delegate;
                if (property is not null)
                {
                    property.DynamicInvoke(args);
                    return;
                }

                if (function == null)
                    throw new Exception("Routine Services is not found in startup class");
            }

            class Start : IStartup
            {
                readonly object value;

                public Start(object value)
                {
                    this.value = value;
                }

                public object Value => value;
            }
        }
    }
}
