using Drivers;
using System.Reflection;

namespace vNext
{
    public interface IDefinition
    {
        IDefinition Device<T>()
            where T : IDevice;

        IDefinition Workflow<T>()
            where T : State;

        IDefinition Workflow(Assembly assembly, Func<Type, bool>? predicate = null);

        IDefinition Startup<T>()
            where T : class;

        IDefinition Startup(object startup);

        /// <summary>
        /// Register failure state where workflow will be navigated in case of unhandled exception.
        /// </summary>
        /// <typeparam name="T">Type of the failure state in workflow</typeparam>
        /// <remarks>
        /// Specefied state should be registered in the workflow as state
        /// </remarks>
        /// <returns></returns>
        IDefinition Failure<T>()
            where T : class;

        IDefinition Entry<T>()
            where T : class;

        /// <summary>
        /// Definition of the application thread.
        /// </summary>
        /// <param name="action">Function to be executed in application thread</param>
        /// <returns></returns>
        IDefinition Run(Func<CancellationToken, Task> action);

        /// <summary>
        /// Final stage of configuring application is to build IApp instance from definition.
        /// </summary>
        /// <returns>Created application as IApp interface</returns>
        IApp Build();
    }
}
