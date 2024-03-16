using System.ComponentModel.DataAnnotations;

namespace Milvasoft.Attributes.Validation;

/// <summary>
/// Specifies that the class or property that this attribute is applied to requires the specified must match the localized regex.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class MilvaRegexAttribute : RegularExpressionAttribute
{
    #region Properties

    /// <summary>
    /// Gets or sets the error message spesific content
    /// </summary>
    public string MemberNameLocalizerKey { get; set; }

    /// <summary>
    /// Gets or sets localized examle regex format.
    /// </summary>
    public string ExampleFormatLocalizerKey { get; set; }

    /// <summary>
    /// Gets or sets property is required.
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Gets or sets error message localization flag.
    /// </summary>
    public bool LocalizeErrorMessages { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor that accepts the maximum length of the string.
    /// </summary>
    public MilvaRegexAttribute() : base(@"^()$") { }

    /// <summary>
    /// Constructor that accepts the maximum length of the string.
    /// </summary>
    /// <param name="pattern"></param>
    public MilvaRegexAttribute(string pattern) : base(pattern) { }

    #endregion

    /// <summary>
    /// Determines whether the specified value of the object is valid.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        var milvaLocalizer = context.GetMilvaLocalizer();

        var localizedPropName = milvaLocalizer == null ? context.MemberName : milvaLocalizer[$"{LocalizerKeys.Localized}{MemberNameLocalizerKey ?? context.MemberName}"];

        if (milvaLocalizer != null)
        {
            if (IsRequired)
            {
                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                {
                    var localizedPattern = milvaLocalizer[$"{LocalizerKeys.RegexPattern}{MemberNameLocalizerKey ?? context.MemberName}"];

                    if (RegexMatcher.MatchRegex(value.ToString(), milvaLocalizer[localizedPattern]))
                        return ValidationResult.Success;
                    else
                    {
                        var exampleFormat = milvaLocalizer[ExampleFormatLocalizerKey ?? LocalizerKeys.RegexExample + context.MemberName];
                        ErrorMessage = milvaLocalizer[LocalizerKeys.RegexErrorMessage, localizedPropName, exampleFormat];
                        return new ValidationResult(FormatErrorMessage(""));
                    }
                }
                else
                {
                    ErrorMessage = milvaLocalizer[LocalizerKeys.PropertyIsRequired, localizedPropName];
                    return new ValidationResult(FormatErrorMessage(""));
                }
            }
            else
            {
                if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
                {
                    var localizedPattern = milvaLocalizer[$"{LocalizerKeys.RegexPattern}{MemberNameLocalizerKey ?? context.MemberName}"];

                    if (RegexMatcher.MatchRegex(value.ToString(), milvaLocalizer[localizedPattern]))
                        return ValidationResult.Success;
                    else
                    {
                        var exampleFormat = milvaLocalizer[ExampleFormatLocalizerKey ?? LocalizerKeys.RegexExample + context.MemberName];
                        ErrorMessage = milvaLocalizer[LocalizerKeys.RegexErrorMessage, localizedPropName, exampleFormat];
                        return new ValidationResult(FormatErrorMessage(""));
                    }
                }
                else
                {
                    return ValidationResult.Success;
                }
            }
        }
        else
        {
            if (base.IsValid(value))
                return ValidationResult.Success;
            else
            {
                ErrorMessage = $"{LocalizerKeys.PleaseEnterAValid} {context.MemberName}";
                return new ValidationResult(FormatErrorMessage(""));
            }
        }
    }
}
