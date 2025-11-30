namespace vNext
{
    partial class App
    {
        partial class Definition
        {
            public IApp Build()
            {
                var app = new App();

                host.ConfigureServices((context, services) =>
                {
                    foreach (var device in devices)
                    {
                        services.AddSingleton(device.Key);
                    }

                    foreach (var state in states)
                    {
                        services.AddTransient(state.Key, state.Value.Type);
                    }

                    services.AddSingleton(app);
                });

                app.Init(host.Build(),
                    devices.Keys);

                return app;
            }
        }
    }
}
