namespace vNext
{
    partial class App
    {
        partial class Definition
        {
            Definition Logging()
            {
                host.ConfigureLogging((hosting, logs) =>
                {
                    logs.AddConfiguration(hosting.Configuration.GetSection("Logging"));
                    logs.AddConsole();
                    logs.AddDebug();
                    logs.AddEventSourceLogger();

                    logs.Configure(options =>
                    {
                        options.ActivityTrackingOptions =
                            ActivityTrackingOptions.SpanId |
                            ActivityTrackingOptions.TraceId |
                            ActivityTrackingOptions.ParentId;
                    });
                });

                return this;
            }
        }
    }
}
