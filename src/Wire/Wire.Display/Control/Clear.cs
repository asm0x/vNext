using ProtoBuf;

namespace Wire
{
    public static partial class Display
    {
        partial class Control
        {
            /// <summary>
            /// Очистка дисплея.
            /// </summary>
            [ProtoContract]
            public class Clear : Control
            {
            }
        }
    }
}