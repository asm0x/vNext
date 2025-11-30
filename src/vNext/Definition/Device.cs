using Drivers;

namespace vNext
{
    partial class App
    {
        partial class Definition
        {
            public IDefinition Device<T>()
                where T : IDevice
            {
                var type = typeof(T);

                if (devices.ContainsKey(type))
                    throw new Exception($"Device type {type} already registered");

                devices.Add(type,
                    new DTI());

                return this;
            }


            /// <summary>
            /// Формируемый набор подключенных устройст.
            /// </summary>
            IDictionary<Type, DTI> devices = new Dictionary<Type, DTI>();

            /// <summary>
            /// Device driver type registration info.
            /// </summary>
            public class DTI
            {
            }
        }
    }
}
