namespace Steam.Web.Interface
{
    /// <summary>
    /// Represents a base contract
    /// </summary>
    public abstract class WebContract
    {
        internal WebContractType Type;
        
        internal WebContract(WebContractType type)
        {
            Type = type;
        }
    }
}
