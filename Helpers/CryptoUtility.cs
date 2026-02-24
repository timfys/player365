
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SmartWinners.Helpers;

/// <summary>
/// Summary description for CryptoUtility
/// </summary>
public static class CryptoUtility
{
	const int ivSize = 16;
	private static byte[] key = new Guid("08E694F1-C188-4A79-B158-768EA3A887FB").ToByteArray();


	public static string EncryptObject<T>(T obj) where T : class
	{
		try
		{
			return EncryptString(JsonConvert.SerializeObject(obj));
		}
		catch
		{
			return null;
		}
	}

	public static T DecryptObject<T>(string encryptedObject) where T : class
	{
		try
		{

			return encryptedObject is null ? null : JsonConvert.DeserializeObject<T>(DecryptString(encryptedObject));
		}
		catch
		{
			return null;
		}
	}

	public static string EncryptString(string text)
	{
		using var aesAlg = Aes.Create();
		using var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV);
		using var msEncrypt = new MemoryStream();
		msEncrypt.Write(aesAlg.IV, 0, ivSize);

		using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
		using (var swEncrypt = new StreamWriter(csEncrypt))
			swEncrypt.Write(text);

		return Convert.ToBase64String(msEncrypt.ToArray());
	}
	public static bool VerifyHMACSHA256(string message, string hmacString, string secretKey)
	{
		byte[] secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
		byte[] messageBytes = Encoding.UTF8.GetBytes(message);

		using var hmac = new HMACSHA256(secretKeyBytes);
		byte[] hmacBytes = hmac.ComputeHash(messageBytes);
		string computedHmacString = BitConverter.ToString(hmacBytes).Replace("-", "").ToLower();

		return (computedHmacString.Equals(hmacString.ToLower()));
	}

	public static string ComputeSHA1Hash(string input)
	{
		using SHA1 sha1 = SHA1.Create();
		var inputBytes = Encoding.UTF8.GetBytes(input);
		var hashBytes = sha1.ComputeHash(inputBytes);

		// Convert the byte array to a hexadecimal string
		var sb = new StringBuilder();
		foreach (var b in hashBytes)
		{
			sb.AppendFormat("{0:x2}", b);
		}

		return sb.ToString();
	}

	public static bool VerifySHA1Hash(string input, string hashToVerify)
	{
		var computedHash = ComputeSHA1Hash(input);

		return string.Equals(computedHash, hashToVerify, StringComparison.OrdinalIgnoreCase);
	}

	public static string DecryptString(string cipherText)
	{
		try
		{
			cipherText = cipherText?.Replace(" ", "+");

			byte[] bytes = Convert.FromBase64String(cipherText);

			using Aes aes = Aes.Create();
			using MemoryStream memoryStream = new(bytes);
			var iv = new byte[16];
			memoryStream.Read(iv, 0, ivSize);

			aes.Key = key;
			aes.IV = iv;
			var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

			using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
			using StreamReader streamReader = new(cryptoStream);
			return streamReader.ReadToEnd();
		}
		catch
		{
			return null;
		}
	}

	public static string GetHmacInHex(string key, string data)
	{
		var hmacKey = Encoding.UTF8.GetBytes(key);

		var dataBytes = Encoding.UTF8.GetBytes(data);

		using var hmac = new HMACSHA256(hmacKey);
		var hash = hmac.ComputeHash(dataBytes);
		return BitConverter.ToString(hash).Replace("-", string.Empty);
	}
}
