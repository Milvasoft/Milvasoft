using Milvasoft.Attributes.Annotations;

namespace Milvasoft.UnitTests.InteceptionTests.SystemLikeNamespaceFixtures;

/// <summary>
/// A user type whose namespace contains "System" (but is not the System namespace).
/// </summary>
public class FileSystemSecretDto
{
    public string Path { get; set; }

    [HideByRole("Hide")]
    public string Secret { get; set; }
}
