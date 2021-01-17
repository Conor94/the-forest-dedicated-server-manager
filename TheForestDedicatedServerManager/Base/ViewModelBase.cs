using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        #region Constructors
        /// <inheritdoc cref="DataErrorBindableBase(Dictionary&lt;string, Func&lt;object, string&lt;&lt; _validators)"/>
        public ViewModelBase(IEventAggregator eventAggregator, IContainerProvider container) : this(eventAggregator, container, null)
        {
        }

        /// <inheritdoc cref="DataErrorBindableBase(Dictionary&lt;string, Func&lt;object, string&lt;&lt; _validators)"/>
        public ViewModelBase(IEventAggregator eventAggregator, IContainerProvider container, Dictionary<string, Func<object, string>> validators) : base(validators)
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
