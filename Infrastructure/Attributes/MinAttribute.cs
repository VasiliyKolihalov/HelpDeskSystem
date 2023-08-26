using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Attributes;

public class MinAttribute : RangeAttribute
{
    public MinAttribute(double value) : base(value, double.MaxValue)
    {
    }

    public MinAttribute(int value) : base(value, int.MaxValue)
    {
    }
}