using States;
using vNext;

var host = App.Create()
    .Workflow(typeof(Start).Assembly)
    .Startup(new
    {
        Services = new Action<IServiceCollection, IConfiguration, IHostEnvironment>((services, configuration, env) =>
        {
            return;
        }),
        Configure = new Action<IConfiguration, ILogger<Program>>((configuration, logs) =>
        {
            return;
        })
    })
    .Entry<Start>()
    .Run(async cancellation =>
    {
        while (!cancellation.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellation);
        }
    })
    .Build();

await host.RunAsync();
