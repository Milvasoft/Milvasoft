namespace Milvasoft.DataAccess.EfCore.Configuration;

/// <summary>
/// Entity auditing configuration. You can register this configuration in startup as singleton. 
/// </summary>
public class AuditConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether to audit the creation date.
    /// </summary>
    public bool AuditCreationDate { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to audit the creator.
    /// </summary>
    public bool AuditCreator { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to audit the modification date.
    /// </summary>
    public bool AuditModificationDate { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to audit the modifier.
    /// </summary>
    public bool AuditModifier { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to audit the deletion date.
    /// </summary>
    public bool AuditDeletionDate { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to audit the deleter.
    /// </summary>
    public bool AuditDeleter { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditConfiguration"/> class.
    /// </summary>
    public AuditConfiguration()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditConfiguration"/> class.
    /// </summary>
    /// <param name="auditCreationDate"></param>
    /// <param name="auditCreator"></param>
    /// <param name="auditModificationDate"></param>
    /// <param name="auditModifier"></param>
    /// <param name="auditDeletionDate"></param>
    /// <param name="auditDeleter"></param>
    public AuditConfiguration(bool auditCreationDate, bool auditCreator, bool auditModificationDate, bool auditModifier, bool auditDeletionDate, bool auditDeleter)
    {
        AuditCreationDate = auditCreationDate;
        AuditCreator = auditCreator;
        AuditModificationDate = auditModificationDate;
        AuditModifier = auditModifier;
        AuditDeletionDate = auditDeletionDate;
        AuditDeleter = auditDeleter;
    }
}
