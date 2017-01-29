namespace TripToPrint.Presenters
{
    public interface IPresenter<out TViewModel, TView>
    {
        TView View { get; }
        TViewModel ViewModel { get; }
        void InitializePresenter(TView view);
    }
}
