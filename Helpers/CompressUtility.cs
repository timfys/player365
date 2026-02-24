using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace SmartWinners.Helpers;

public class CompressUtility
{
    public static byte[] CompressString(string inputString) {
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);

    using var outputStream = new MemoryStream();
    using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
    {
      gzipStream.Write(inputBytes, 0, inputBytes.Length);
    }

    return outputStream.ToArray();
  }

    public static string DecompressString(byte[] compressedBytes) {
    using var inputStream = new MemoryStream(compressedBytes);
    using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
    using var outputMemoryStream = new MemoryStream();
    gzipStream.CopyTo(outputMemoryStream);
    byte[] outputBytes = outputMemoryStream.ToArray();
    return Encoding.UTF8.GetString(outputBytes);
  }

    public static string UrlCompressObject(object obj)
    {
        return HttpUtility.UrlEncode(Convert.ToBase64String(CompressString(JsonConvert.SerializeObject(obj))));
    }
    public static T UrlDeCompressObject <T>(string obj)
    {
        var urlDecoded = HttpUtility.UrlDecode(obj);
        urlDecoded = urlDecoded.Replace(" ", "+");
        var bytes = Convert.FromBase64String(urlDecoded);
        var decodedObjJsonString = DecompressString(bytes);
        return JsonConvert.DeserializeObject<T>(decodedObjJsonString);
    }

    public static string CompressStringExtended(string originalText)
    {
    using var memoryStream = new MemoryStream();
    using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
    using (var writer = new StreamWriter(deflateStream, Encoding.UTF8))
    {
      writer.Write(originalText);
    }

    var compressedBytes = memoryStream.ToArray();
    var compressedBase64 = Convert.ToBase64String(compressedBytes);

    return compressedBase64;
  }
    public static string DecompressStringExtended(string compressedBase64)
    {
        byte[] compressedBytes = Convert.FromBase64String(compressedBase64);
    using var compressedStream = new MemoryStream(compressedBytes);
    using var decompressedStream = new MemoryStream();
    using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
    deflateStream.CopyTo(decompressedStream);

    var decompressedText = Encoding.UTF8.GetString(decompressedStream.ToArray());
    return decompressedText;
  }
}