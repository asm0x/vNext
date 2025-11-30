namespace vNext
{
    partial class App
    {
        partial class Definition
        {
            Definition Hosting()
            {
                host.ConfigureHostConfiguration(config =>
                {
                    config.AddEnvironmentVariables(prefix: "DOTNET_");
                });

                return this;
            }
        }
    }
}
