using GoldCasino.ApiModule.Services.BusinessApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

namespace GoldCasino.ApiModule.Mapping;
public static class EntityMapper
{
	public static TTarget MapTo<TTarget>(Entity entity)
			where TTarget : new()
	{
		var target = new TTarget();
		var targetProps = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance);

		// Словарь: "dbFieldName" → PropertyInfo
		var entityProps = typeof(Entity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.ToDictionary(
						p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? p.Name,
						p => p
				);

		foreach (var targetProp in targetProps)
		{
			var fieldAttr = targetProp.GetCustomAttribute<EntityFieldAttribute>();
			if (fieldAttr == null) continue;

			if (entityProps.TryGetValue(fieldAttr.FieldName, out var entityProp) &&
					entityProp.PropertyType == targetProp.PropertyType)
			{
				var value = entityProp.GetValue(entity);
				targetProp.SetValue(target, value);
			}
		}

		return target;
	}

	public static (T1, T2) MapTo<T1, T2>(Entity entity)
			where T1 : new()
			where T2 : new()
	{
		return (MapTo<T1>(entity), MapTo<T2>(entity));
	}

	public static (T1, T2, T3) MapTo<T1, T2, T3>(Entity entity)
			where T1 : new()
			where T2 : new()
			where T3 : new()
	{
		return (MapTo<T1>(entity), MapTo<T2>(entity), MapTo<T3>(entity));
	}
}