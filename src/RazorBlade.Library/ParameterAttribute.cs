namespace RazorBlade;

/// <summary>
/// Marks a property as a component parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ParameterAttribute : Attribute
{
}
