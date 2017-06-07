using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace TripToPrint
{
    [ExcludeFromCodeCoverage]
    public sealed class NormalCursor : IDisposable
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