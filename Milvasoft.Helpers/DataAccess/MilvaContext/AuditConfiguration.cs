using Milvasoft.Helpers.DependencyInjection;

namespace Milvasoft.Helpers.DataAccess.MilvaContext
{
    /// <summary>
    /// Entity auditing configuration. You can register this configuration in startup as singleton.
    /// </summary>
    public record AuditConfiguration(bool AuditCreationDate, bool AuditCreator, bool AuditModificationDate, bool AuditModifier, bool AuditDeletionDate, bool AuditDeleter) : IAuditConfiguration
    {
    }
}
