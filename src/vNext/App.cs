using Drivers;

namespace vNext
{
    public partial class App : IApp
    {
        public IReadOnlyDictionary<Type, Device>? Devices { get; private set; }

        public event Action? Exit;

        IHost? host;

        private App()
        {
        }

        private void Init(IHost host, ICollection<Type> devices)
        {
            this.host = host;

            var s = new Dictionary<Type, Device>();

            foreach (var device in devices)
            {
                var instance = host.Services.GetRequiredService(device) as Device;
                if (instance == null)
                    throw new Exception($"Failed to build device {device}");

                s[device] = instance;
            }

            host.Services.GetService<IHostApplicationLifetime>()
                ?.ApplicationStopping.Register(() =>
                {
                    Exit?.Invoke();
                });

            Devices = s;
        }


        public static IDefinition Create()
        {
            return Definition.Create();
        }

        public Task RunAsync(CancellationToken token) =>
            host.RunAsync(token);

        public Task RunAsync() =>
            host.RunAsync();

        public void Stop() =>
            host?.StopAsync().Wait();
    }
}
