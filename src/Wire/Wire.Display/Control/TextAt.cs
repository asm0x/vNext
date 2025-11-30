using ProtoBuf;

namespace Wire
{
    public static partial class Display
    {
        partial class Control
        {
            /// <summary>
            /// Отображение текста на дисплее в заданной позиции.
            /// </summary>
            [ProtoContract]
            public class TextAt : Control
            {
                /// <summary>
                /// Текст для отображения.
                /// </summary>
                [ProtoMember(1)]
                public string Value { get; set; }

                /// <summary>
                /// Координата отображения по X.
                /// </summary>
                [ProtoMember(2)]
                public int X { get; set; }

                /// <summary>
                /// Координата отображения по Y.
                /// </summary>
                [ProtoMember(3)]
                public int Y { get; set; }
            }
        }
    }
}