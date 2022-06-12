using Milvasoft.Core.Abstractions;

namespace Milvasoft.DataAccess.EfCore.Utils;

/// <summary>
/// Entity auditing configuration. You can register this configuration in startup as singleton.
/// </summary>
public record AuditConfiguration(bool AuditCreationDate,
                                 bool AuditCreator,
                                 bool AuditModificationDate,
                                 bool AuditModifier,
                                 bool AuditDeletionDate,
                                 bool AuditDeleter) : IAuditConfiguration
{ }
