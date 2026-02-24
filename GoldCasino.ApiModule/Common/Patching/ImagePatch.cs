namespace GoldCasino.ApiModule.Common.Patching;

public sealed class ImagePatch
{
	private readonly Dictionary<string, string> _images = new(StringComparer.Ordinal);

	public ImagePatch Set(string column, string value)
	{
		_images[column] = value ?? string.Empty;
		return this;
	}

	public ImagePatch Clear(string column)
	{
		_images[column] = string.Empty;
		return this;
	}

	public bool IsEmpty => _images.Count == 0;

	public (string[] Names, string[] Values) ToArrays()
	{
		var names = new string[_images.Count];
		var values = new string[_images.Count];
		int i = 0;
		foreach (var kv in _images)
		{
			names[i] = kv.Key;
			values[i] = kv.Value ?? string.Empty;
			i++;
		}
		return (names, values);
	}
}
