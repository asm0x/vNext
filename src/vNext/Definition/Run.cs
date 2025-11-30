namespace vNext
{
    public interface IRun
    {
        Func<CancellationToken, Task> Action { get; }
    }

    partial class App
    {
        partial class Definition
        {
            bool _run = false;

            public IDefinition Run(Func<CancellationToken, Task> action)
            {
                if (_run)
                    throw new Exception("Run can be used only once");

                host.ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IRun>(new Fibre(action));

                });

                _run = true;

                return this;
            }

            class Fibre : IRun
            {
                Func<CancellationToken, Task> action;

                public Fibre(Func<CancellationToken, Task> action)
                {
                    this.action = action;
                }

                public Func<CancellationToken, Task> Action => action;
            }
        }
    }
}
