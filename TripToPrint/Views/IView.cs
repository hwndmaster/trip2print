namespace TripToPrint.Views
{
    public interface IView<TPresenter>
    {
        TPresenter Presenter { get; set; }
        object DataContext { get; set; }
    }
}
