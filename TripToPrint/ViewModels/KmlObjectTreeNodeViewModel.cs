using System.Collections.ObjectModel;
using TripToPrint.Core.Models;

namespace TripToPrint.ViewModels
{
    public abstract class KmlObjectTreeNodeViewModel : ViewModelBase
    {
        private bool _enabled;
        private bool _suspendEnablingOfChildren;

        public abstract string Name { get; }
        public abstract IKmlElement Element { get; }

        public bool IsPartOfRoute { get; set; }
        public KmlObjectTreeNodeViewModel Parent { get; set; }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (value == _enabled)
                    return;
                _enabled = value;
                OnPropertyChanged();

                if (!_suspendEnablingOfChildren)
                {
                    foreach (var child in Children)
                    {
                        child.Enabled = value;
                    }
                }

                if (Parent != null && value && !Parent.Enabled)
                {
                    Parent.EnableOnlySelf();
                }
            }
        }

        public ObservableCollection<KmlObjectTreeNodeViewModel> Children { get; }
            = new ObservableCollection<KmlObjectTreeNodeViewModel>();

        public void EnableOnlySelf()
        {
            _suspendEnablingOfChildren = true;
            Enabled = true;
            _suspendEnablingOfChildren = false;
        }
    }
}
