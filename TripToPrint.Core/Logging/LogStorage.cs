using System;
using System.Collections.Generic;

namespace TripToPrint.Core.Logging
{
    public interface ILogStorage
    {
        void ClearAll();
        void WriteLog(LogItem item);

        event EventHandler<LogItem> ItemAdded;
        event EventHandler AllItemsRemoved;
    }

    public class LogStorage : ILogStorage
    {
        private readonly List<LogItem> _items = new List<LogItem>();

        public void ClearAll()
        {
            _items.Clear();

            AllItemsRemoved?.Invoke(this, null);
        }

        public void WriteLog(LogItem item)
        {
            _items.Add(item);

            ItemAdded?.Invoke(this, item);
        }

        public event EventHandler<LogItem> ItemAdded;
        public event EventHandler AllItemsRemoved;
    }
}
