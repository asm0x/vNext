using ProtoBuf;
using System.Runtime.Serialization;

namespace Wire
{
    public static partial class Display
    {
        partial class Control
        {
            /// <summary>
            /// Отображение текста на дисплее.
            /// </summary>
            [ProtoContract]
            public class Text : Control
            {
                /// <summary>
                /// Текст для отображения.
                /// </summary>
                [ProtoMember(1)]
                public string Value { get; set; }
            }
        }
    }
}