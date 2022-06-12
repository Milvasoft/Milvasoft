namespace Milvasoft.Core.Utils.Models;

/// <summary>
/// Invalid strings for prevent hacking or someting ;)
/// </summary>
public class InvalidString
{
    /// <summary>
    /// Name of invalid string.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Invalid values.
    /// </summary>
    public List<string> Values { get; set; }
}
