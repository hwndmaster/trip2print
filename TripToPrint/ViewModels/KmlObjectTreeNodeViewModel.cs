using System;
using System.Collections.ObjectModel;
using TripToPrint.Core.Models;

namespace TripToPrint.ViewModels
{
    public abstract class KmlObjectTreeNodeViewModel : ViewModelBase
    {
        private bool _suspendEnablingOfChildren;

        protected KmlObjectTreeNodeViewModel()
        {
            EnabledChanged += (sender, enabled) => HandleEnabledChanged();
        }

        public abstract string Name { get; }
        public abstract IKmlElement Element { get; }

        public bool IsPartOfRoute { get; set; }
        public KmlObjectTreeNodeViewModel Parent { get; set; }

        public event EventHandler<bool> EnabledChanged;

        public bool Enabled
        {
            get => GetOrDefault<bool>();
            set => RaiseAndSetIfChanged(value, EnabledChanged);
        }

        public ObservableCollection<KmlObjectTreeNodeViewModel> Children { get; }
            = new ObservableCollection<KmlObjectTreeNodeViewModel>();

        public void EnableOnlySelf()
        {
            _suspendEnablingOfChildren = true;
            Enabled = true;
            _suspendEnablingOfChildren = false;
        }

        private void HandleEnabledChanged()
        {
            if (!_suspendEnablingOfChildren)
            {
                foreach (var child in Children)
                {
                    child.Enabled = Enabled;
                }
            }

            if (Parent != null && Enabled && !Parent.Enabled)
            {
                Parent.EnableOnlySelf();
            }
        }
    }
}
