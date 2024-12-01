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

/// <summary>
/// Represents the options for no lock interception.
/// </summary>
public class HaveDbContextType : IHaveDbContextType
{
    private Type _dbContextType;
    private string _dbContextAssemblyQualifiedName;

    /// <inheritdoc/>
    public string DbContextAssemblyQualifiedName
    {
        get => _dbContextAssemblyQualifiedName;
        set
        {
            if (value is not null)
            {
                _dbContextAssemblyQualifiedName = value;
                _dbContextType = Type.GetType(_dbContextAssemblyQualifiedName);
            }
        }
    }

    /// <inheritdoc/>
    public Type DbContextType
    {
        get => _dbContextType;
        set
        {
            if (value is not null)
            {
                _dbContextType = value;
                _dbContextAssemblyQualifiedName = value.AssemblyQualifiedName;
            }
        }
    }

    /// <inheritdoc/>
    public Type GetDbContextType() => _dbContextType;
}
