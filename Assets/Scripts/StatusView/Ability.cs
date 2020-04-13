using System;

[AttributeUsage(AttributeTargets.Property)]
public class AbilityAttribute : Attribute
{
    public string Name { get; }

    public int Order { get; }

    public AbilityAttribute(string name, int order = 0)
    {
        Name = name;
        Order = order;
    }
}