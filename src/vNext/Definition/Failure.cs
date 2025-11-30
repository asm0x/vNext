namespace vNext
{
    public interface IFailure
    {
        Type Type { get; }
    }

    partial class App
    {
        partial class Definition
        {
            public IDefinition Failure<T>()
                where T : class
            {
                host.ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IFailure>(new Crash<T>());

                });

                return this;
            }

            class Crash<T> : IFailure
            {
                public Type Type => typeof(T);
            }
        }
    }
}
