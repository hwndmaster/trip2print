using System;
using System.Windows.Input;

namespace TripToPrint
{
    public class NormalCursor : IDisposable
    {
        private readonly Cursor _previousCursor;

        public NormalCursor()
        {
            _previousCursor = Mouse.OverrideCursor;

            Mouse.OverrideCursor = Cursors.Arrow;
        }

        public void Dispose()
        {
            Mouse.OverrideCursor = _previousCursor;
        }
    }
}