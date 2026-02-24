using GoldCasino.ApiModule.Common.Patching;

namespace GoldCasino.ApiModule.Extensions;
public static class PatchApplyExtensions
{

	/// <summary>
	/// Apply patch arrays to any request object with arbitrary property names.
	/// Example usage:
	///   patch.ApplyTo(req, (r,n,v) => { r.NamesArray = n; r.ValuesArray = v; });
	/// or:
	///   patch.ApplyTo(req, (r,n,v) => { r.FieldNames = n; r.FieldValues = v; });
	/// </summary>
	public static TRequest ApplyTo<T, TRequest>(
		this Patch<T> patch,
		TRequest request,
		Action<TRequest, string[], string[]> binder,
		Func<object?, string>? formatter = null)
	{
		var (names, values) = patch.ToArrays(formatter);
		binder(request, names, values);
		return request;
	}
}
