using System;
using System.Collections.Generic;

namespace TripToPrint.Core.Logging
{
    public interface ILogStorage
    {
        void Clear(LogCategory category);
        void WriteLog(LogItem item);

        event EventHandler<LogItem> ItemAdded;
        event EventHandler<LogCategory> CategoryItemsRemoved;
    }

    public class LogStorage : ILogStorage
    {
        private readonly List<LogItem> _items = new List<LogItem>();

        public void Clear(LogCategory category)
        {
            _items.Clear();

            CategoryItemsRemoved?.Invoke(this, category);
        }

        public void WriteLog(LogItem item)
        {
            _items.Add(item);

            ItemAdded?.Invoke(this, item);
        }

        public event EventHandler<LogItem> ItemAdded;
        public event EventHandler<LogCategory> CategoryItemsRemoved;
    }
}
