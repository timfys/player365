namespace GoldCasino.ApiModule.Mapping;

[AttributeUsage(AttributeTargets.Property)]
public class EntityFieldAttribute(string fieldName) : Attribute
{
	public string FieldName { get; } = fieldName;
}