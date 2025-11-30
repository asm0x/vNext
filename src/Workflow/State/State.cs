using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vNext
{
    /// <summary>
    /// Базовый класс для состояния системы.
    /// </summary>
    public class State : IState
    {
        /// <summary>
        /// Запущенные задачи состояния.
        /// </summary>
        public Dictionary<int, CancellationTokenSource>? tasks = null;

        Workflow IState.Workflow
        {
            get { return workflow; }
            set
            {
                workflow = value;
            }
        }
        Workflow workflow;


        /// <summary>
        /// Событие активации состояния.
        /// </summary>
        /// <param name="data">Опционально переданные данные</param>
        public virtual Result Activate(object? data)
        {
            return Result.OK;
        }

        /// <summary>
        /// Ввод данных в данном состоянии системы.
        /// </summary>
        /// <param name="data">Данные ввода</param>
        public virtual Result IO(object data)
        {
            return Result.OK;
        }

        /// <summary>
        /// Перемещение workflow в заданное состояние.
        /// </summary>
        /// <typeparam name="T">Состояние</typeparam>
        /// <param name="data">Опционально данные для состояния</param>
        public Result Navigate<T>(object? data = null)
        {
            return workflow.Navigate(typeof(T), data);
        }

        /// <summary>
        /// Обработка операции.
        /// </summary>
        /// <param name="control"></param>
        public void Device(object control)
        {
            workflow.Device(control);
        }
        
        /// <summary>
        /// Запуск ассинхронного действия.
        /// </summary>
        /// <param name="action">Действие для выполнения</param>
        protected Task Work(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return Work(() =>
            {
                action();

                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Запуск ассинхронного действия.
        /// </summary>
        /// <param name="action">Действие для выполнения</param>
        protected Task Work(Func<Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var token = new CancellationTokenSource();
            if (tasks == null)
                tasks = new Dictionary<int, CancellationTokenSource>();

            var task = Task.Run(async () =>
            {
                await action();
            },
            token.Token);

            tasks[task.GetHashCode()] = token;

            return task;
        }

        /// <summary>
        /// Завершение ассинхронного действия.
        /// </summary>
        /// <param name="action">Работа для завершения</param>
        protected void Cancel(Task work)
        {
            if (tasks is not null &&
                tasks.TryGetValue(work.GetHashCode(), out var token))
            {
                token.Cancel();
            }
        }

        /// <summary>
        /// Событие выхода из состояния.
        /// </summary>
        public virtual void Dispose()
        {
            if (tasks is not null)
            {
                foreach (var token in tasks.Values)
                    token.Cancel();

                tasks = null;
            }
        }

    }
}
