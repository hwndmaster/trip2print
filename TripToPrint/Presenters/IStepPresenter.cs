using System.Threading.Tasks;

namespace TripToPrint.Presenters
{
    public interface IStepPresenter
    {
        Task Activated();
        bool ValidateToGoBack();
        bool ValidateToGoNext();
        void GetBackNextTitles(ref string back, ref string next);
    }
}
