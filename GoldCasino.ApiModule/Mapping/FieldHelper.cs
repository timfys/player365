using System.Collections.Concurrent;
using System.Reflection;

namespace GoldCasino.ApiModule.Mapping;

public static class FieldHelper
{
	private static readonly ConcurrentDictionary<string, string[]> _cache = new();

	public static string[] GetFieldsFor(params Type[] dtoTypes)
	{
		var key = string.Join("|", dtoTypes.Select(t => t.FullName));

		return _cache.GetOrAdd(key, _ =>
		{
			return dtoTypes
					.SelectMany(t => t.GetProperties()
							.Select(p => p.GetCustomAttribute<EntityFieldAttribute>()?.FieldName))
					.Where(f => f != null)
					.Distinct()
					.ToArray()!;
		});
	}
}

public static class FieldHelper<T>
{
	public static readonly string[] Fields;

	static FieldHelper()
	{
		Fields = typeof(T).GetProperties()
				.Select(p => p.GetCustomAttribute<EntityFieldAttribute>()?.FieldName)
				.Where(f => f != null)
				.ToArray()!;
	}
}
public static class FieldHelper<T1, T2>
{
	public static readonly string[] Fields;

	static FieldHelper()
	{
		Fields = typeof(T1).GetProperties()
				.Select(p => p.GetCustomAttribute<EntityFieldAttribute>()?.FieldName)
				.Concat(typeof(T2).GetProperties()
				.Select(p => p.GetCustomAttribute<EntityFieldAttribute>()?.FieldName))
				.Where(f => f != null)
				.Distinct()
				.ToArray()!;
	}
}

public static class FieldHelper<T1, T2, T3>
{
	public static readonly string[] Fields;

	static FieldHelper()
	{
		Fields = typeof(T1).GetProperties()
				.Select(p => p.GetCustomAttribute<EntityFieldAttribute>()?.FieldName)
				.Concat(typeof(T2).GetProperties()
				.Select(p => p.GetCustomAttribute<EntityFieldAttribute>()?.FieldName))
				.Concat(typeof(T3).GetProperties()
				.Select(p => p.GetCustomAttribute<EntityFieldAttribute>()?.FieldName))
				.Where(f => f != null)
				.Distinct()
				.ToArray()!;
	}
}