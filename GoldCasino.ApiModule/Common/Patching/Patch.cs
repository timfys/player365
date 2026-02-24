using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;

namespace GoldCasino.ApiModule.Common.Patching;

public sealed class Patch<T>
{
	private readonly Dictionary<string, object?> _fields = new(StringComparer.Ordinal);

	public Patch<T> Set<TProp>(Expression<Func<T, TProp>> property, TProp value)
	{
		_fields[ColumnName(property)] = value;
		return this;
	}

	public Patch<T> Clear<TProp>(Expression<Func<T, TProp>> property)
	{
		_fields[ColumnName(property)] = null; // PATCH: clear with empty string later
		return this;
	}

	/// <summary>Use when you must target a column that has no property.</summary>
	public Patch<T> SetRaw(string column, object? value)
	{
		_fields[column] = value;
		return this;
	}

	/// <summary>Returns aligned arrays for any SOAP request (global use).</summary>
	public (string[] Names, string[] Values) ToArrays(Func<object?, string>? formatter = null)
	{
		var fmt = formatter ?? PatchFormatter.Soap;
		var names = new string[_fields.Count];
		var values = new string[_fields.Count];

		int i = 0;
		foreach (var kv in _fields)
		{
			names[i] = kv.Key;
			values[i] = fmt(kv.Value);
			i++;
		}
		return (names, values);
	}

	// — column name resolution ------------------------------------------------

	private static readonly ConcurrentDictionary<MemberInfo, string> _nameCache = new();

	private static string ColumnName<TProp>(Expression<Func<T, TProp>> expr)
	{
		var member = ExtractMember(expr.Body)
			?? throw new ArgumentException("Expression must be a property access.", nameof(expr));

		return _nameCache.GetOrAdd(member, m =>
		{
			// Priority: JsonPropertyName → DataMember(Name) → Column(Name) → Property.Name
			if (m.GetCustomAttribute<JsonPropertyNameAttribute>() is { } jp) return jp.Name;

			if (m.GetCustomAttribute<System.Runtime.Serialization.DataMemberAttribute>() is { Name: { Length: > 0 } dn })
				return dn;

			if (m.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>() is { Name: { Length: > 0 } cn })
				return cn;

			return m.Name;
		});
	}

	private static MemberInfo? ExtractMember(Expression body)
	{
		if (body is MemberExpression m1) return m1.Member;
		if (body is UnaryExpression u && u.Operand is MemberExpression m2) return m2.Member;
		return null;
	}
}
