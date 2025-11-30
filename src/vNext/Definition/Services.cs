namespace vNext
{
    partial class App
    {
        partial class Definition
        {
            Definition Services()
            {
                host.ConfigureServices(services =>
                {
                    services.AddHostedService<Init>();

                    services.AddSingleton<Workflow>();

                });

                return this;
            }
        }
    }
}
