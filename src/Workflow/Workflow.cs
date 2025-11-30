using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace vNext
{
    public class Workflow
    {
        /// <summary>
        /// Механизм синхронизации.
        /// </summary>
        public readonly Sync sync = new Sync();

        /// <summary>
        /// Зарегистрированные типы команд устройств.
        /// </summary>
        readonly Dictionary<Type, List<Action<object>>> controls = new();

        /// <summary>
        /// Контейнер зависимостей.
        /// </summary>
        readonly IServiceProvider services;

        /// <summary>
        /// Логирование.
        /// </summary>
        readonly ILogger<Workflow> logs;

        /// <summary>
        /// Текущее состояние workflow.
        /// </summary>
        State? state = null;

        /// <summary>
        /// Тип состояния к которому система будет приходить в случае ошибок работы.
        /// </summary>
        Type? failure = null;


        public Workflow(IServiceProvider services,
            ILogger<Workflow> logs)
        {
            this.services = services;
            this.logs = logs;

        }

        /// <summary>
        /// Текущее состояние workflow.
        /// </summary>
        public State? State
        {
            get { return state; }
        }

        /// <summary>
        /// Routine to instantiate state by its type.
        /// </summary>
        /// <param name="services">Services provider to be used</param>
        /// <param name="type">Type of the state to create</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        State Instance(IServiceProvider services, Type type)
        {
            var state = services.GetRequiredService(type) as State;
            if (state == null)
                throw new ArgumentException($"Invalid state {type}");

            (state as IState).Workflow = this;

            return state;
        }

        /// <summary>
        /// Перемещение workflow в заданное состояние.
        /// </summary>
        /// <param name="type">Состояние</param>
        /// <param name="data">Опционально данные для состояния</param>
        /// <remarks>
        /// First navigate without context of other navigate creates new services scope in DI.
        /// </remarks>
        public Result Navigate(Type type, object? data = null)
        {
            try
            {
                State? next = null;
                try
                {
                    next = Instance(services, type);
                }
                catch (Exception e)
                {
                    logs.LogCritical("Failed to instantiate state {type}: {failure}",
                        type.GetType(),
                        e.Message);

                    throw;
                }

                if (state is not null)
                    try
                    {
                        state.Dispose();
                    }
                    catch (Exception e)
                    {
                        logs.LogCritical("Failed to leave state {state}: {failure}",
                            state.GetType(),
                            e.Message);

                        throw;
                    }

                state = next;

                logs.LogDebug("State: {state}", next.GetType());
                try
                {
                    return state.Activate(data);
                }
                catch (Exception e)
                {
                    logs.LogError(e, "Failed to activate state {state}: {failure}",
                        state.GetType(),
                        e.Message);

                    throw;
                }
            }
            catch
            {
                if (failure is not null)
                {
                    state = Instance(services, failure);
                    try
                    {
                        return state.Activate(data);
                    }
                    catch (Exception e)
                    {
                        logs.LogError(e, "Unable to activate failure state {type}: {failure}",
                            state.GetType(),
                            e.Message);
                    }
                }

                return Result.Fail;
            }
        }

        /// <summary>
        /// Направление ввода в активное состояние.
        /// </summary>
        /// <param name="data">Ввод</param>
        public Result IO(object data)
        {
            if (data == null)
                return Result.OK;

            if (state == null)
                return Result.Fail;

            using (sync.Lock())
            {
                return state.IO(data);
            }
        }

        /// <summary>
        /// Send control to device.
        /// </summary>
        /// <param name="control">Control to send</param>
        public void Device(object control)
        {
            var type = control.GetType();

            var devices = controls
                .Where(x => type.IsSubclassOf(x.Key) || type == x.Key)
                .Select(x => x.Value);

            foreach (var device in devices)
                foreach (var action in device)
                    try
                    {
                        action(control);
                    }
                    catch (Exception e)
                    {
                        logs.LogError(e, "Failed to send '{control}' to {device}: {failure}",
                            control.GetType(),
                            device.GetType(),
                            e.Message);
                    }
        }

        /// <summary>
        /// Регистрация обработчиков выполнения команд.
        /// </summary>
        /// <typeparam name="T">Тип, для которого регистрируется обработка</typeparam>
        /// <param name="control">Команда для выполнения</param>
        public Action Control<T>(Action<object> control)
            where T : class
        {
            return Control(typeof(T), control);
        }

        /// <summary>
        /// Регистрация обработчиков выполнения команд.
        /// </summary>
        /// <param name="type">Тип, для которого регистрируется обработка</param>
        /// <param name="control">Команда для выполнения</param>
        public Action Control(Type type, Action<object> control)
        {
            if (!controls.TryGetValue(type, out var list))
                controls[type] = list = new List<Action<object>>();

            list.Add(control);

            return () =>
            {
                list.Remove(control);
            };
        }

        /// <summary>
        /// Set failure state of the workflow.
        /// </summary>
        /// <param name="state">Type of state</param>
        public void SetFailure(Type state)
        {
            failure = state;
        }
    }
}
