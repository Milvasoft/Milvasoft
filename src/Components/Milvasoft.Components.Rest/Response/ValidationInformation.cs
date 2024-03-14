using System.Collections;
using System.Runtime.Serialization;

namespace Milvasoft.Components.Rest.Response;

/// <summary>
/// Represents a collection of validation items used to store validation information.
/// </summary>
[DataContract]
public class ValidationInformation : ICollection<ValidationItem>
{
    [DataMember]
    private List<ValidationItem> _errors;

    /// <summary>
    /// Gets the number of validation items in the collection.
    /// </summary>
    public int Count => _errors.Count;

    /// <summary>
    /// Gets a value indicating whether the collection is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationInformation"/> class.
    /// </summary>
    public ValidationInformation() => _errors = [];

    public void Add(string key, string errorMessage) => _errors.Add(new ValidationItem() { Key = key, ValidationMessage = errorMessage });

    public void Add(ValidationItem item) => _errors.Add(item);

    public IEnumerator<ValidationItem> GetEnumerator() => _errors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _errors.GetEnumerator();

    void ICollection<ValidationItem>.Clear() => _errors.Clear();

    bool ICollection<ValidationItem>.Contains(ValidationItem item) => _errors.Contains(item);

    void ICollection<ValidationItem>.CopyTo(ValidationItem[] array, int arrayIndex) => _errors.CopyTo(array, arrayIndex);

    bool ICollection<ValidationItem>.Remove(ValidationItem item) => _errors.Remove(item);
}
