namespace Milvasoft.DataAccess.EfCore.Utils.Enums;

/// <summary>
/// Enumerates the choices for saving changes in a database context.
/// </summary>
public enum SaveChangesChoice
{
    /// <summary>
    /// Represents saving changes after every database operation.
    /// </summary>
    AfterEveryOperation,

    /// <summary>
    /// Represents manual saving of changes where the user is responsible for calling the save method.
    /// </summary>
    Manual
}