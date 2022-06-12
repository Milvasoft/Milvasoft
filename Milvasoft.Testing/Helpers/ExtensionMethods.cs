using Milvasoft.Core.Exceptions;

namespace Milvasoft.Testing.Helpers;

/// <summary>
/// Extension methods.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// checks whether the <paramref name="data"/> data is null.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="data"></param>
    /// <param name="message"></param>
    public static void IsNull<TObject>(this TObject data, string message) => _ = data == null ? throw new MilvaTestException(message) : true;

    /// <summary>
    /// Applies <seealso cref="string.Trim()"/> to the list.
    /// </summary>
    /// <param name="datas"></param>
    public static void Trim(this List<string> datas)
    {
        for (int i = 0; i < datas.Count; i++)
            datas[i] = datas[i].Trim();
    }
}
