using System;

namespace SmartWinners.Models.BusinessAPI;

	[AttributeUsage(AttributeTargets.Property)]
	public class UpdateFieldAttribute(string fieldName) : Attribute
	{
    public string FieldName { get; } = fieldName;
}
