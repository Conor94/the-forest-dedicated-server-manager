using Prism.Events;
using Prism.Ioc;
using TheForestDedicatedServerManager.Events;

namespace TheForestDedicatedServerManager.Base
{
    /// <summary>
    /// Abstract base class for view models. Inherits from <see cref="DataErrorBindableBase"/>.
    /// </summary>
    public abstract class ViewModelBase : DataErrorBindableBase
    {
        #region Fields and properties
        private IContainerProvider mContainer;
        private IEventAggregator mEventAggregator;
        private string mTitle;

        public IContainerProvider Container
        {
            get => mContainer;
            private set => mContainer = value;
        }
        public IEventAggregator EventAggregator
        {
            get => mEventAggregator;
            private set => mEventAggregator = value;
        }
        public string Title
        {
            get => mTitle;
            set => SetProperty(ref mTitle, value);
        }
        #endregion

        #region Constructor
        public ViewModelBase(IEventAggregator eventAggregator, IContainerProvider container)
        {
            Container = container;
            EventAggregator = eventAggregator;
            Title = "";
        }
        #endregion

        #region Methods
        public void InvokeViewChangeRequest()
        {
            EventAggregator.GetEvent<ViewChangeRequestEvent>().Publish(this);
        }
        #endregion
    }
}
