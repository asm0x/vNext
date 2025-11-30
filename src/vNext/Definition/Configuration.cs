using System.Reflection;

namespace vNext
{
    partial class App
    {
        partial class Definition
        {
            Definition Configuration()
            {
                host.ConfigureAppConfiguration((hosting, config) =>
                    {
                        var env = hosting.HostingEnvironment;

                        config.AddJsonFile("settings.json", optional: true, true)
                                .AddJsonFile($"settings.{env.EnvironmentName}.json", optional: true, true);

                        if (env.IsDevelopment())
                        {
                            var assembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                            if (assembly is not null)
                            {
                                config.AddUserSecrets(assembly, optional: true, true);
                            }
                        }

                        config.AddEnvironmentVariables();
                    });

                return this;
            }
        }
    }
}
