using System.Globalization;
using System.Reflection;

namespace vNext
{
    partial class App
    {
        partial class Definition
        {
            public class Init : BackgroundService
            {
                readonly App app;
                readonly Workflow workflow;
                readonly IServiceProvider services;
                readonly ILogger<Init> logs;

                public Init(App app,
                    Workflow workflow,
                    IServiceProvider services,
                    ILogger<Init> logs)
                {
                    this.app = app;
                    this.workflow = workflow;
                    this.services = services;
                    this.logs = logs;

                    Configure(services);
                }

                static void Configure(IServiceProvider services)
                {
                    var startup = services.GetService<IStartup>();
                    if (startup is not null)
                    {
                        var value = startup.Value;
                        var routine = value.GetType().GetMethod("Configure");
                        if (routine is not null)
                        {
                            routine.Invoke(value, Args(services, routine.GetParameters()));
                            return;
                        }

                        var property = value.GetType().GetProperty("Configure")?.GetValue(value) as Delegate;
                        if (property is not null)
                        {
                            property.DynamicInvoke(Args(services, property.Method.GetParameters()));
                            return;
                        }

                        if (routine == null)
                            throw new Exception("Configure routine is not found in startup class");
                    }
                }

                static object[] Args(IServiceProvider services, ParameterInfo[] args)
                {
                    var data = new object[args.Length];

                    for (var index = 0; index < args.Length; index++)
                    {
                        var arg = args[index];
                        try
                        {
                            data[index] = services.GetRequiredService(arg.ParameterType);
                        }
                        catch (Exception e)
                        {
                            throw new Exception(string.Format(CultureInfo.InvariantCulture,
                                "Could not resolve a service of type '{0}' for the parameter '{1}' of Configure routine.",
                                arg.ParameterType.FullName,
                                arg.Name),
                                e);
                        }
                    }

                    return data;
                }

                protected override async Task ExecuteAsync(CancellationToken cancellation)
                {
                    if (app.Devices == null)
                        throw new Exception("Devices are not initialised");

                    var failure = services.GetService<IFailure>();
                    if (failure is not null)
                    {
                        workflow.SetFailure(failure.Type);
                    }

                    var entry = services.GetService<IEntry>();
                    if (entry == null)
                    {
                        app.Stop();
                        return;
                    }

                    foreach (var device in app.Devices.Values)
                    {
                        workflow.Control(device.Control, data => device.Send(data));

                        device.Received += data =>
                            workflow.IO(data);
                    }

                    services.CreateScope();
                    workflow.Navigate(entry.Type);

                    var run = services.GetService<IRun>();
                    if (run is not null)
                        await run.Action(cancellation);
                }
            }
        }
    }
}