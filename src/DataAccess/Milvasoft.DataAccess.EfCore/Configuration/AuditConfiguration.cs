namespace Milvasoft.DataAccess.EfCore.Configuration;

/// <summary>
/// Entity auditing configuration. You can register this configuration in startup as singleton. 
/// </summary>
public record AuditConfiguration(bool AuditCreationDate = true,
                                 bool AuditCreator = false,
                                 bool AuditModificationDate = true,
                                 bool AuditModifier = false,
                                 bool AuditDeletionDate = true,
                                 bool AuditDeleter = false)
{
}
