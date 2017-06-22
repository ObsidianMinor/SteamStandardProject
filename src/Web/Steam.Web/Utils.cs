using System.Linq;

namespace Steam.Web
{
    internal static class Utils
    {
        internal static string CreateQueryString(params (string key, object value)[] values)
        {
            var nullChecked = values.Where(kv => !string.IsNullOrWhiteSpace(kv.key) && kv.value != null);

            return "?format=json&" + string.Join("&", nullChecked.Select(c => $"{c.key}={c.value}"));
        }
    }
}
