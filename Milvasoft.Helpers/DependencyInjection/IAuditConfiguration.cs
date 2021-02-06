namespace Milvasoft.Helpers.DependencyInjection
{
    /// <summary>
    /// Configuration for entity auditing. You can register this configuration in startup as singleton.
    /// </summary>
    public interface IAuditConfiguration
    {
        /// <summary>
        /// Determines whether the entity auditing by the creation date.
        /// </summary>
        public bool AuditCreationDate { get; init; }

        /// <summary>
        /// Determines whether the entity auditing by the creator.
        /// </summary>
        public bool AuditCreator { get; init; }

        /// <summary>
        /// Determines whether the entity auditing by the modification date.
        /// </summary>
        public bool AuditModificationDate { get; init; }

        /// <summary>
        /// Determines whether the entity auditing by the modifier.
        /// </summary>
        public bool AuditModifier { get; init; }

        /// <summary>
        /// Determines whether the entity auditing by the deletion date.
        /// </summary>
        public bool AuditDeletionDate { get; init; }

        /// <summary>
        /// Determines whether the entity auditing by the deleter.
        /// </summary>
        public bool AuditDeleter { get; init; }
    }
}
