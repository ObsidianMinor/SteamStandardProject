using System;
using System.Collections.Generic;
using System.Text;

namespace Steam.Web.Interface
{
    public enum WebContractType
    {
        /// <summary>
        /// An interface contract
        /// </summary>
        Interface,
        /// <summary>
        /// A method contract
        /// </summary>
        Method,
        /// <summary>
        /// A parameter contract
        /// </summary>
        Parameter,
        /// <summary>
        /// A return parameter contract
        /// </summary>
        Return
    }
}
