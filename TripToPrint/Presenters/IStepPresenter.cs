using System;
using System.Threading.Tasks;

namespace TripToPrint.Presenters
{
    public interface IStepPresenter
    {
        Task Activated();
        bool BeforeToGoBack();
        Task<bool> BeforeGoNext();
        void GetBackNextTitles(ref string back, ref string next);

        event EventHandler GoNextRequested;
    }
}
