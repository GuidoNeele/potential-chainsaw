namespace RazorBlade;

/// <summary>
/// Marks a Razor component for code generation.
/// The source generator will create a strongly-typed wrapper class for this component.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class GenerateComponentAttribute : Attribute
{
}
