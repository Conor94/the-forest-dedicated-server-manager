using Prism.Events;
using System;
using System.Collections.Generic;
using TheForestDedicatedServerManager.Events;

namespace TheForestDedicatedServerManager.Base
{
    /// <summary>
    /// Abstract base class for view models. Inherits from <see cref="DataErrorBindableBase"/>.
    /// </summary>
    public abstract class ViewModelBase : DataErrorBindableBase
    {
        #region Fields and properties
        private string mTitle;
        private IEventAggregator mEventAggregator;
        public string Title
        {
            get => mTitle;
            set
            {
                SetProperty(ref mTitle, value);
            }
        }
        public IEventAggregator EventAggregator
        {
            get => mEventAggregator;
            private set => mEventAggregator = value;
        }

        #endregion

        #region Constructors
        /// <inheritdoc cref="DataErrorBindableBase(Dictionary&lt;string, Func&lt;object, string&lt;&lt; _validators)"/>
        public ViewModelBase(IEventAggregator eventAggregator) : this(eventAggregator, null)
        {
        }

        /// <inheritdoc cref="DataErrorBindableBase(Dictionary&lt;string, Func&lt;object, string&lt;&lt; _validators)"/>
        public ViewModelBase(IEventAggregator eventAggregator, Dictionary<string, Func<object, string>> validators) : base(validators)
        {
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
