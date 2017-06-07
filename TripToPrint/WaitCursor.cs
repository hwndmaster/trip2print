using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace TripToPrint
{
    [ExcludeFromCodeCoverage]
    public class WaitCursor : IDisposable
    {
        private readonly Cursor _previousCursor;

        public WaitCursor()
        {
            _previousCursor = Mouse.OverrideCursor;

            Mouse.OverrideCursor = Cursors.Wait;
        }

        public void Dispose()
        {
            Mouse.OverrideCursor = _previousCursor;
        }
    }
}
