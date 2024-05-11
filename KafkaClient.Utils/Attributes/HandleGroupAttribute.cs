
namespace KafkaClient.Utils.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class HandleGroupAttribute(string group) : Attribute
{
    public string Group { get; } = group;
}
