﻿using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.MultiLanguage.EntityBases;

namespace Milvasoft.DataAccess.EfCore.Utils.LookupModels;

/// <summary>
/// For request content. 
/// </summary>
public class LookupRequest
{
    /// <summary>
    /// Requested parameters for lookups.
    /// </summary>
    public List<LookupRequestParameter> Parameters { get; set; }
}

/// <summary>
/// Lookup request parameters.
/// </summary>
public class LookupRequestParameter
{
    /// <summary>
    /// Requested entity name.
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// Requested properties.
    /// </summary>
    public List<string> RequestedPropertyNames { get; set; }

    /// <summary>
    /// Filter criterias.
    /// </summary>
    public FilterRequest Filtering { get; set; }

    /// <summary>
    /// Sort criterias.
    /// </summary>
    public SortRequest Sorting { get; set; }

    /// <summary>
    /// Updates the filter criteria for translation property names.
    /// </summary>
    /// <param name="translationPropertyNames">The list of translation property names.</param>
    /// <returns>The updated filter criteria.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "IsNullOrEmpty is checked null")]
    public void UpdateFilterByForTranslationPropertyNames(List<string> translationPropertyNames)
    {
        var criterias = Filtering?.Criterias;

        if (criterias.IsNullOrEmpty() || translationPropertyNames.IsNullOrEmpty())
            return;

        for (var i = 0; i < criterias.Count; i++)
        {
            var criteria = criterias[i];

            if (translationPropertyNames.Contains(criteria.FilterBy))
                criteria.FilterBy = criteria.GetFilterByAsListFormat(MultiLanguageEntityPropertyNames.Translations);
        }
    }
}
