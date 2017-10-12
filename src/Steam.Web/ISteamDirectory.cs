using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steam.Web
{
    public interface ISteamDirectory
    {
        [WebMethod(Name = "GetCMList")]
        Task<WebResponse<ConnectionManagerList>> GetConnectionManagerListAsync(long cellid, long? maxcount = null);

        [WebMethod(Name = "GetCSList")]
        Task<WebResponse<ContentServerList>> GetContentServersAsync(long cellid, long? maxCount = null);
    }
}
