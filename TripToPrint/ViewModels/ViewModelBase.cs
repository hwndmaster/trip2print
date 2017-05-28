using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TripToPrint.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        private readonly ConcurrentDictionary<string, object> _propertyBag = new ConcurrentDictionary<string, object>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected T GetOrDefault<T>(T defaultValue = default(T), [CallerMemberName] string name = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var result = _propertyBag.GetOrAdd(name, _ => defaultValue);

            return (T)result;
        }

        protected void RaiseAndSetIfChanged<T>(T value, EventHandler<T> eventHandler = null, [CallerMemberName] string name = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            object oldValue;
            _propertyBag.TryGetValue(name, out oldValue);

            if (Equals(oldValue, value))
            {
                return;
            }

            _propertyBag.AddOrUpdate(name, _ => value, (_, __) => value);
            OnPropertyChanged(name);

            eventHandler?.Invoke(this, value);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
