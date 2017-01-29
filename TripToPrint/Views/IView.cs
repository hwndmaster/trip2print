namespace TripToPrint.Views
{
    public interface IView<TPresenter, TViewModel>
    {
        TPresenter Presenter { get; set; }
        TViewModel ViewModel { get; set; }
        void BindData();
        void Refresh();
    }
}
