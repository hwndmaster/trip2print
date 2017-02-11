using System;
using System.Threading.Tasks;

namespace TripToPrint.Presenters
{
    public interface IStepPresenter
    {
        Task Activated();
        bool BeforeToGoBack();
        bool BeforeGoNext();
        void GetBackNextTitles(ref string back, ref string next);

        event EventHandler GoNextRequested;
    }
}
