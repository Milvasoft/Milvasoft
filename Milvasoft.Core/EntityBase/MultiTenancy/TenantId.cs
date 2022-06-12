using Milvasoft.Core.Exceptions;
using Milvasoft.Core.Extensions;
using System.ComponentModel;

namespace Milvasoft.Core.EntityBase.MultiTenancy;

/// <summary>
/// Fully unique tenant id.
/// </summary>
[TypeConverter(typeof(TenantIdTypeConverter))]
[Serializable]
public struct TenantId : IEquatable<TenantId>
{
    /// <summary>
    /// Creates an empty <see cref="TenantId"/> instance.
    /// </summary>
    public static readonly TenantId Empty = new();

    private static readonly string _invalidTenantIdErrorMessage = "This string is not convertible to TenantId";
    private readonly string _tenancyName;
    private int _branchNo;
    private readonly string _hash;

    /// <summary>
    /// Unique tenancy name of tenant.
    /// </summary>
    public string TenancyName { get => _tenancyName; }

    /// <summary>
    /// Branch no of Tenant.
    /// </summary>
    public int BranchNo { get => _branchNo; }

    /// <summary>
    /// Creates new instance of <see cref="TenantId"/>.
    /// </summary>
    /// <param name="tenantIdString"></param>
    public TenantId(string tenantIdString)
    {
        if (TryParse(tenantIdString))
        {
            var splitted = tenantIdString.Split('_');

            var tenancyName = splitted[0];

            var branchNo = Convert.ToInt32(splitted[1]);

            _tenancyName = tenancyName.ToLowerInvariant();
            _branchNo = branchNo;
            _hash = $"{tenancyName}_{branchNo}".HashToString();
        }
        else throw new MilvaDeveloperException(_invalidTenantIdErrorMessage)
        {
            ExceptionCode = (int)MilvaException.InvalidTenantId
        };
    }

    /// <summary>
    /// Creates new instance of <see cref="TenantId"/>.
    /// </summary>
    /// <param name="tenancyName"></param>
    /// <param name="branchNo"></param>
    public TenantId(string tenancyName, int branchNo)
    {
        if (string.IsNullOrWhiteSpace(tenancyName))
            throw new MilvaUserFriendlyException("TenancyNameRequired", MilvaException.TenancyNameRequired);

        if (branchNo <= 0)
            throw new MilvaDeveloperException("Branch No cannot be 0 or less.");

        _tenancyName = tenancyName.ToLowerInvariant();
        _branchNo = branchNo;
        _hash = $"{tenancyName}_{branchNo}".HashToString();
    }

    /// <summary>
    /// Generates new unique tenant id.
    /// </summary>
    /// <returns></returns>
    public static TenantId NewTenantId()
    {
        var guid = Guid.NewGuid();

        string guidString = Convert.ToBase64String(guid.ToByteArray());

        guidString = guidString.Replace("=", "");

        guidString = guidString.Replace("+", "");

        return new TenantId(guidString, 1);
    }

    /// <summary>
    /// Combines Tenancy Name and BranchNo into a hash code.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => HashCode.Combine(TenancyName, BranchNo);

    /// <summary>
    /// Returns hash.
    /// </summary>
    /// <returns></returns>
    public string GetHashString() => $"{_tenancyName}_{_branchNo}".HashToString();

    /// <summary>
    /// Compares hashes.
    /// </summary>
    /// <param name="other"></param>
    /// <returns> If both hash are equals returns true, othewise false. </returns>
    public override bool Equals(object other)
    {
        TenantId tenantId;

        // Check that o is a TenantId first
        if (other == null || other is not TenantId id)
            return false;
        else tenantId = id;

        if (tenantId._hash != GetHashString())
            return false;

        return true;
    }

    /// <summary>
    /// Compares hashes.
    /// </summary>
    /// <param name="other"></param>
    /// <returns> If both hash are equals returns true, othewise false. </returns>
    public bool Equals(TenantId other)
    {
        if (other._hash != GetHashString())
            return false;

        return true;
    }

    /// <summary>
    /// Compares <see cref="TenancyName"/> with <paramref name="other"/>.TenancyName.
    /// </summary>
    /// <param name="other"></param>
    /// <returns> If both TenancyName are equals returns true, othewise false. </returns>
    public bool TenancyNameEquals(TenantId other)
    {
        if (TenancyName != other.TenancyName)
            return false;

        return true;
    }

    /// <summary>
    /// Compares <see cref="BranchNo"/> with <paramref name="other"/>.BranchNo.
    /// </summary>
    /// <param name="other"></param>
    /// <returns> If both BranchNo are equals returns true, othewise false. </returns>
    public bool BranchNoEquals(TenantId other)
    {
        if (BranchNo != other.BranchNo)
            return false;

        return true;
    }

    /// <summary>
    /// Returns Tenancy Name and Branch No with '_' seperator 
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{_tenancyName}_{_branchNo}";

    /// <summary>
    /// <para>Supported formats : G , H </para>
    /// <para>G returns tenancyName and branchNo with '_' seperator</para>
    /// <para>H returns tenancyName and branchNo and hash with '_' seperator</para>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public string ToString(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
            format = "G";

        return format.ToUpperInvariant() switch
        {
            "G" => $"{_tenancyName}_{_branchNo}",
            "H" => $"{_tenancyName}_{_branchNo}_{_hash}",
            _ => throw new FormatException(string.Format("The {0} format string is not supported.", format)),
        };
    }

    /// <summary>
    /// Parses <paramref name="str"/> to <see cref="TenantId"/>.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static TenantId Parse(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            throw new MilvaDeveloperException(_invalidTenantIdErrorMessage)
            {
                ExceptionCode = (int)MilvaException.InvalidTenantId
            };

        if (TryParse(str))
        {
            var splitted = str.Split('_');

            return new TenantId(splitted[0], Convert.ToInt32(splitted[1]));
        }
        else throw new MilvaDeveloperException(_invalidTenantIdErrorMessage)
        {
            ExceptionCode = (int)MilvaException.InvalidTenantId
        };
    }

    /// <summary>
    /// Converts the string representation of a <see cref="TenantId"/> to its string equivalent.
    /// A return value indicates whether the conversion succeeded.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool TryParse(string str)
    {
        var splittedArray = str.Split('_');

        if (splittedArray.Length < 2) return false;

        if (string.IsNullOrWhiteSpace(splittedArray[0]) || string.IsNullOrWhiteSpace(splittedArray[1]))
            return false;
        try
        {
            Convert.ToInt32(splittedArray[1]);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Provides implicit casting.
    /// </summary>
    /// <param name="tenantId"></param>
    public static implicit operator string(TenantId tenantId) => tenantId.ToString();

    /// <summary>
    /// Provides explicit casting.
    /// </summary>
    /// <param name="tenantId"></param>
    public static explicit operator TenantId(string tenantId)
    {
        if (!TryParse(tenantId))
            throw new MilvaDeveloperException(_invalidTenantIdErrorMessage)
            {
                ExceptionCode = (int)MilvaException.InvalidTenantId
            };

        return Parse(tenantId);
    }

    /// <summary>
    /// Indicates whether the values of two specified <see cref="TenantId"/> objects are equal.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator ==(TenantId a, TenantId b)
    {
        if (a._hash != b._hash)
            return false;

        return true;
    }

    /// <summary>
    /// Indicates whether the values of two specified <see cref="TenantId"/> objects are not equal.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator !=(TenantId a, TenantId b) => !(a == b);

    /// <summary>
    /// Indicates whether the values of <paramref name="a"/>.BranchNo are smaller than <paramref name="b"/>.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator <(TenantId a, TenantId b)
    {
        if (a.BranchNo > b.BranchNo)
            return false;

        return true;
    }

    /// <summary>
    /// Indicates whether the values of <paramref name="a"/>.BranchNo are bigger than <paramref name="b"/>.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator >(TenantId a, TenantId b) => !(a <= b);

    /// <summary>
    /// Indicates whether the values of <paramref name="a"/>.BranchNo are smaller or equal than <paramref name="b"/>.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator <=(TenantId a, TenantId b)
    {
        if (a.BranchNo > b.BranchNo)
            return false;

        return true;
    }

    /// <summary>
    /// Indicates whether the values of <paramref name="a"/>.BranchNo are bigger or equal than <paramref name="b"/>.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator >=(TenantId a, TenantId b) => !(a <= b);

    /// <summary>
    /// Increases <paramref name="tenantId"/>.BranchNo.
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public static TenantId operator ++(TenantId tenantId)
    {
        tenantId._branchNo++;
        return tenantId;
    }

    /// <summary>
    /// Decreases <paramref name="tenantId"/>.BranchNo.
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public static TenantId operator --(TenantId tenantId)
    {
        tenantId._branchNo--;
        return tenantId;
    }
}
