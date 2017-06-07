using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace TripToPrint.Services
{
    public interface IProcessService
    {
        void Start(string fileName, string arguments = null);
    }

    [ExcludeFromCodeCoverage]
    public sealed class ProcessService : IProcessService
    {
        public void Start(string fileName, string arguments = null)
        {
            Process.Start(fileName, arguments);
        }
    }
}
