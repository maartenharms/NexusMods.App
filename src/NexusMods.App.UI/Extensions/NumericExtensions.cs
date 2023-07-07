namespace NexusMods.App.UI.Extensions;

public static class NumericExtensions
{
    /// <summary>
    /// Returns true if the value is within the tolerance of the other value.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="other"></param>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    public static bool EqualsWithTolerance(this double value, double other, double tolerance) => Math.Abs(value - other) < tolerance;
    
    /// <summary>
    /// Returns true if the value is within the tolerance of the other value, using the default tolerance of 0.0000001.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool EqualsWithTolerance(this double value, double other) => EqualsWithTolerance(value, other, 0.0000001);
}
