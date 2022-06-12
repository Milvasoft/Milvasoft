namespace Milvasoft.Core.Utils.Models;

/// <summary>
/// Order by properties for multiple ordey by in database.
/// </summary>
public class OrderByProp
{
    /// <summary>
    /// Determines order by Property name of entity.
    /// </summary>
    public virtual string PropName { get; set; }

    /// <summary>
    /// Determines order by ascending or descending.
    /// </summary>
    public virtual bool Ascending { get; set; }

    /// <summary>
    /// Priority of order operation. Ex. first order by creation date then order by updated date.
    /// </summary>
    public virtual int Priority { get; set; }
}
