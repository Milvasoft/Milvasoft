namespace Milvasoft.Interception.Ef.Builder;

/// <summary>
/// Represents the options for no lock interception.
/// </summary>
public interface IHaveDbContextType
{
    /// <summary>
    /// DbContext assembly qualified name for configuring options from configuration file.
    /// For example 'SomeNamespace.SomeContext, SomeNamespace, Version=8.0.0.0, Culture=neutral, PublicKeyToken=null'
    /// </summary>
    public string DbContextAssemblyQualifiedName { get; set; }

    /// <summary>
    /// DbContext type. For example typeof(SomeContext)
    /// </summary>
    public Type DbContextType { get; set; }

    /// <summary>
    /// Returns the dbcontext type to be applied to the transaction.
    /// </summary>
    /// <returns>Returns the dbcontext type to be applied to the transaction.</returns>
    public Type GetDbContextType();
}

public class HaveDbContextType : IHaveDbContextType
{
    protected Type _dbContextType;
    protected string _dbContextAssemblyQualifiedName;

    public string DbContextAssemblyQualifiedName
    {
        get => _dbContextAssemblyQualifiedName;
        set
        {
            _dbContextAssemblyQualifiedName = value;
            _dbContextType = Type.GetType(_dbContextAssemblyQualifiedName);
        }
    }

    public Type DbContextType
    {
        get => _dbContextType;
        set
        {
            _dbContextType = value;
            _dbContextAssemblyQualifiedName = value.AssemblyQualifiedName;
        }
    }

    public Type GetDbContextType() => _dbContextType;
}
