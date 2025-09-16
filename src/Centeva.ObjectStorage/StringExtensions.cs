using System.Text;
using System.Web;

namespace Centeva.ObjectStorage;

public static class StringExtensions
{
    /// <summary>
    /// Decodes a BASE64 encoded string
    /// </summary>
    public static string Base64Decode(this string stringToDecode)
    {
        byte[] data = Convert.FromBase64String(stringToDecode);

        return Encoding.UTF8.GetString(data, 0, data.Length);
    }

    /// <summary>
    /// Decode a URL-encoded string
    /// </summary>
    /// <param name="stringToDecode"></param>
    /// <returns></returns>
    public static string? UrlDecode(this string? stringToDecode)
    {
        return HttpUtility.UrlDecode(stringToDecode);
    }
}
