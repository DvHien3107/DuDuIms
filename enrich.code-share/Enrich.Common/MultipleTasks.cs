using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Common
{
    public class MultipleTasks<T>
    {
        public T Owner { get; }

        private readonly Type _ownerType;
        private readonly MultipleTasks _tasks;

        public MultipleTasks(T owner)
        {
            Owner = owner;

            _ownerType = typeof(T);
            _tasks = new MultipleTasks();
        }

        public void Add(string mappingPropName, Task execute) => _tasks.Add(execute, val =>
        {
            var prop = _ownerType.GetProperty(mappingPropName);
            prop?.SetValue(Owner, val);
        });

        public void Add(Task execute, Action<object> done = null) => _tasks.Add(execute, done);

        public async Task WhenAll(bool clearTasks = true) => await _tasks.WhenAll(clearTasks);

        public bool HasTasks => _tasks.HasTasks;
    }

    public class MultipleTasks
    {
        private readonly List<(Task Execute, Action<object> Done)> _tasks = new List<(Task, Action<object>)>();

        public bool HasTasks => _tasks.Count > 0;

        public void Add(Task execute, Action<object> done = null) => _tasks.Add((execute, done));

        public void AddRange(params (Task Execute, Action<object> Done)[] tasks) => _tasks.AddRange(tasks);

        public async Task WhenAll(bool clearTasks = true)
        {
            if (_tasks.Count == 0)
            {
                return;
            }

            await Task.WhenAll(_tasks.Select(a => a.Execute));

            foreach (var item in _tasks.Where(a => a.Done != null))
            {
                var value = Value(item.Execute);
                item.Done?.Invoke(value);
            }

            if (clearTasks)
            {
                _tasks.Clear();
            }
        }

        public async Task<IEnumerable<object>> WhenAllValues()
        {
            if (_tasks.Count == 0)
            {
                return Enumerable.Empty<object>();
            }

            await Task.WhenAll(_tasks.Select(a => a.Execute));

            return _tasks.Select(item => Value(item.Execute));
        }

        private object Value(Task task) => task.GetType().GetProperty("Result")?.GetValue(task);
    }
}
