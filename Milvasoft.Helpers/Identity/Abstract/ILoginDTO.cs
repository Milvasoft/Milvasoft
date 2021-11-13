namespace Milvasoft.Helpers.Identity.Abstract;

/// <summary>
/// Login and sign up processes are happens with this dto.
/// </summary>
public interface ILoginDTO
{
    /// <summary>
    /// UserName of user.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Email of user.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Password of user.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Remember me option.
    /// </summary>
    public bool Persistent { get; set; }

}
