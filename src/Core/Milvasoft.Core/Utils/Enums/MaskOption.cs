namespace Milvasoft.Core.Utils.Enums;

/// <summary>
/// Represents the options for applying a mask in a string.
/// </summary>
public enum MaskOption : byte
{
    /// <summary>
    /// Apply the mask at the beginning of the string.
    /// </summary>
    AtTheBeginingOfString = 1,

    /// <summary>
    /// Apply the mask in the middle of the string.
    /// </summary>
    InTheMiddleOfString = 2,

    /// <summary>
    /// Apply the mask at the end of the string.
    /// </summary>
    AtTheEndOfString = 3
}
