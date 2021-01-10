using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace TheForestDedicatedServerManager.Base
{
    /// <summary>
    /// Inherits from <see cref="BindableBase"/> and implements the <see cref="IDataErrorInfo"/> interface.
    /// </summary>
    public abstract class DataErrorBindableBase : BindableBase, IDataErrorInfo
    {
        #region Fields and properties
        private Dictionary<string, Func<object, string>> mValidators;
        private string mError;

        private Dictionary<string, Func<object, string>> Validators
        {
            get => mValidators ?? (mValidators = new Dictionary<string, Func<object, string>>());
            set => mValidators = value;
        }
        public string this[string propertyName]
        {
            get
            {
                if (Validators.ContainsKey(propertyName))
                {
                    Type t = GetType();
                    PropertyInfo pInfo = t.GetProperty(propertyName);
                    Error = Validators[propertyName].Invoke(pInfo.GetValue(this));
                    return Error;
                }
                else
                {
                    throw new ArgumentException($"Property name could not be found in the {nameof(Validators)} dictionary.");
                }
            }
        }
        public string Error
        {
            get => mError;
            set => mError = value;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for the <see cref="DataErrorBindableBase"/> class.
        /// </summary>
        public DataErrorBindableBase() : this(null) { }

        /// <summary>
        /// Constructor for the <see cref="DataErrorBindableBase"/> class.
        /// </summary>
        /// <param name="_validators">Holds methods for validating properties (using the <see cref="IDataErrorInfo/>"/> interface).</param>
        public DataErrorBindableBase(Dictionary<string, Func<object, string>> _validators)
        {
            Validators = _validators;
        }
        #endregion

        #region Methods
        public void AddValidator(string propertyName, Func<object, string> validatorFunc)
        {
            Validators.Add(propertyName, validatorFunc);
        }
        #endregion
    }
}
