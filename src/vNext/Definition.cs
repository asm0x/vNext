namespace vNext
{
    partial class App
    {
        partial class Definition : IDefinition
        {
            readonly IHostBuilder host = new HostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory());

            public static IDefinition Create()
            {
                return new Definition()
                    .Hosting()
                    .Configuration()
                    .Logging()
                    .IoC()
                    .Services();
            }
        }
    }
}