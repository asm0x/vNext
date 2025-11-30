using System.Reflection;

namespace vNext
{
    partial class App
    {
        partial class Definition
        {
            public IDefinition Workflow<T>()
                where T : State
            {
                Add(typeof(T));

                return this;
            }

            public IDefinition Workflow(Assembly assembly, Func<Type, bool>? predicate = null)
            {
                foreach (var type in assembly.DefinedTypes
                    .Where(x => !x.IsAbstract && !x.IsGenericTypeDefinition))
                {
                    if (null != predicate && !predicate!(type))
                        continue;

                    Add(type);
                }

                return this;
            }


            void Add(Type type)
            {
                var types = new List<Type>();

                var i = type;
                while (i is not null)
                {
                    types.Add(i);

                    i = i.BaseType;
                    if (i == typeof(State))
                        break;
                }

                var level = types.Count;

                foreach (var t in types)
                {
                    if (!states.TryGetValue(t, out var exist) ||
                        exist.Level <= level)
                        states[t] = new STI(type, level);
                }
            }

            /// <summary>
            /// Формируемый набор состояний workflow для регистрации.
            /// </summary>
            IDictionary<Type, STI> states = new Dictionary<Type, STI>();

            /// <summary>
            /// Данные состояний для регистрации.
            /// </summary>
            class STI
            {
                /// <summary>
                /// Тип реализации.
                /// </summary>
                public Type Type { get; set; }

                /// <summary>
                /// Уровень наследования типа.
                /// </summary>
                public int Level { get; set; }


                public STI(Type type, int level)
                {
                    Type = type;
                    Level = level;
                }
            }
        }
    }
}
