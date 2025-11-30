namespace Drivers
{
    public abstract partial class Device : IDevice
    {
        public event Action<object>? Received;

        public abstract Type Control { get; }
        public abstract Type Events { get; }


        public abstract void Send(object data);

        public void Receive(object data)
        {
            Received?.Invoke(data);
        }
    }
}
