using System.Linq;

namespace Steam.Web
{
    internal static class Utils
    {
        internal static string CreateQueryString(ResponseFormat format, params (string Key, string Value)[] values)
        {
            var nullChecked = values.Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && kv.Value != null);

            return $"?format={format.ToString().ToLower()}&" + string.Join("&", nullChecked.Select(c => $"{c.Key}={c.Value}"));
        }
    }
}
