using System;
using System.Collections.Generic;

namespace TheForestDedicatedServerManager.Base
{
    public abstract class ModelBase : DataErrorBindableBase
    {
        #region Constructors
        public ModelBase() : base()
        {
        }

        public ModelBase(Dictionary<string, Func<object, string>> validators) : base(validators)
        {
        }
        #endregion
    }
}
