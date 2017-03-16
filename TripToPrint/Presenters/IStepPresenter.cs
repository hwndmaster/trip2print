using System.Threading.Tasks;

namespace TripToPrint.Presenters
{
    public interface IStepPresenter
    {
        Task Activated();
        Task<bool> BeforeGoBack();
        Task<bool> BeforeGoNext();
        void GetBackNextTitles(ref string back, ref string next);
    }
}
