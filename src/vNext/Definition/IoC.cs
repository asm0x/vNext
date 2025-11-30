namespace vNext
{
    partial class App
    {
        partial class Definition
        {
            Definition IoC()
            {
                host.UseDefaultServiceProvider((context, options) =>
                {
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        options.ValidateOnBuild = true;
                        options.ValidateScopes = true;
                    }
                });

                return this;
            }
        }
    }
}
