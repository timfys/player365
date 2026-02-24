using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GoldCasino.ApiModule.Helpers;

public class CookieEncryptionHelper(IOptions<CookieAuthOptions> opts)
{
	private readonly CookieAuthOptions _settings = opts.Value;

	public string Encrypt(object payload)
	{
		using var doc = JsonDocument.Parse(JsonSerializer.Serialize(payload));
		var root = doc.RootElement.EnumerateObject()
				.ToDictionary(p => p.Name, p => p.Value);
		root["keyVersion"] = JsonDocument.Parse(_settings.CurrentKeyVersion.ToString()).RootElement;
		root["schemaVersion"] = JsonDocument.Parse(_settings.SchemaVersion.ToString()).RootElement;

		var allJson = JsonSerializer.Serialize(root);
		var plain = Encoding.UTF8.GetBytes(allJson);

		var key = Convert.FromBase64String(_settings.Keys[_settings.CurrentKeyVersion]);

		var iv = RandomNumberGenerator.GetBytes(12);
		var cipher = new byte[plain.Length];
		var tag = new byte[16];
		using var aes = new AesGcm(key, tag.Length);
		aes.Encrypt(iv, plain, cipher, tag);

		var blob = new byte[iv.Length + tag.Length + cipher.Length];
		Buffer.BlockCopy(iv, 0, blob, 0, iv.Length);
		Buffer.BlockCopy(tag, 0, blob, iv.Length, tag.Length);
		Buffer.BlockCopy(cipher, 0, blob, iv.Length + tag.Length, cipher.Length);

		return Convert.ToBase64String(blob);
	}

	public T Decrypt<T>(string token)
	{
		var blob = Convert.FromBase64String(token);

		var iv = blob.Take(12).ToArray();
		var tag = blob.Skip(12).Take(16).ToArray();
		var cipher = blob.Skip(28).ToArray();

		var key = Convert.FromBase64String(_settings.Keys[_settings.CurrentKeyVersion]);
		var plain = new byte[cipher.Length];
		using var aes = new AesGcm(key, tag.Length);
		aes.Decrypt(iv, cipher, tag, plain);

		var json = Encoding.UTF8.GetString(plain);
		var doc = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;

		if (doc.TryGetValue("schemaVersion", out var sv) &&
				sv.GetInt32() != _settings.SchemaVersion)
		{
			throw new InvalidOperationException("Old schema");
		}

		doc.Remove("keyVersion");
		doc.Remove("schemaVersion");

		var payloadJson = JsonSerializer.Serialize(doc);
		return JsonSerializer.Deserialize<T>(payloadJson)!;
	}

	public bool TryDecrypt<T>(string token, out T? result)
	{
		result = default;

		try
		{
			var blob = Convert.FromBase64String(token);

			var iv = blob.Take(12).ToArray();
			var tag = blob.Skip(12).Take(16).ToArray();
			var cipher = blob.Skip(28).ToArray();

			var key = Convert.FromBase64String(_settings.Keys[_settings.CurrentKeyVersion]);
			var plain = new byte[cipher.Length];
			using var aes = new AesGcm(key, tag.Length);
			aes.Decrypt(iv, cipher, tag, plain);

			var json = Encoding.UTF8.GetString(plain);
			var doc = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;

			if (doc.TryGetValue("schemaVersion", out var sv) &&
					sv.GetInt32() != _settings.SchemaVersion)
			{
				return false;
			}

			doc.Remove("keyVersion");
			doc.Remove("schemaVersion");

			var payloadJson = JsonSerializer.Serialize(doc);
			result = JsonSerializer.Deserialize<T>(payloadJson)!;
			return true;
		}
		catch
		{
			return false;
		}
	}
}
