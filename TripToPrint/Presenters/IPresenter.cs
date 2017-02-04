namespace TripToPrint.Presenters
{
    public interface IPresenter<TViewModel, TView>
    {
        TView View { get; }
        TViewModel ViewModel { get; }
        void InitializePresenter(TView view, TViewModel viewModel = default(TViewModel));
    }
}
