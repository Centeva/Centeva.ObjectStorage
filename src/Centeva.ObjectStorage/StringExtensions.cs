using System.Text;
using System.Web;

namespace Centeva.ObjectStorage;

public static class StringExtensions
{
    /// <summary>
    /// Encodes a string to BASE64 format
    /// </summary>
    public static string Base64Encode(this string stringToEncode)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(stringToEncode));
    }

    /// <summary>
    /// Decodes a BASE64 encoded string
    /// </summary>
    public static string Base64Decode(this string stringToDecode)
    {
        byte[] data = Convert.FromBase64String(stringToDecode);

        return Encoding.UTF8.GetString(data, 0, data.Length);
    }

    /// <summary>
    /// URL-encodes a string
    /// </summary>
    /// <param name="stringToEncode"></param>
    /// <returns></returns>
    public static string? UrlEncode(this string? stringToEncode)
    {
        return HttpUtility.UrlEncode(stringToEncode);
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
