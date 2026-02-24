using System.Reflection;

namespace GoldCasino.ApiModule.Mapping;
public static class DtoMapper
{
	public static TTarget MapTo<TTarget, TSource>(TSource source)
					where TTarget : new()
	{
		var srcProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
		var destProps = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance);

		var target = new TTarget();

		foreach (var destProp in destProps)
		{
			var srcProp = srcProps.FirstOrDefault(p => p.Name == destProp.Name
																								 && p.PropertyType == destProp.PropertyType);
			if (srcProp != null)
			{
				var value = srcProp.GetValue(source);
				destProp.SetValue(target, value);
			}
		}

		return target;
	}

	public static (T1, T2, T3) Split<TSource, T1, T2, T3>(TSource source)
			where T1 : new()
			where T2 : new()
			where T3 : new()
	{
		return (
				MapTo<T1, TSource>(source),
				MapTo<T2, TSource>(source),
				MapTo<T3, TSource>(source)
		);
	}

	public static (T1, T2) Split<TSource, T1, T2>(TSource source)
			where T1 : new()
			where T2 : new()
	{
		return (
				MapTo<T1, TSource>(source),
				MapTo<T2, TSource>(source)
		);
	}
}
