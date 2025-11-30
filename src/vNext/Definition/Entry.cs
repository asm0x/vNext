namespace vNext
{
    public interface IEntry
    {
        Type Type { get; }
    }

    partial class App
    {
        partial class Definition
        {
            bool _entry = false;

            public IDefinition Entry<T>()
                where T : class
            {
                if (_entry)
                    throw new Exception("Entry can be used only once");

                host.ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IEntry>(new Main<T>());

                });

                _entry = true;

                return this;
            }

            class Main<T> : IEntry
            {
                public Type Type => typeof(T);
            }
        }
    }
}
