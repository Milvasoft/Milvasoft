using FluentAssertions;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using Milvasoft.Core.Exceptions;
using System.Text.Json;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.RequestTests;

/// <summary>
/// Clients may only filter, sort and aggregate by allowed properties; In/NotIn filters are applied; row counts are capped.
/// </summary>
[Trait("Rest Components Unit Tests", "Milvasoft.Components.Rest project unit tests.")]
public class RequestPropertyPolicyTests
{
    [Theory]
    [InlineData(nameof(PolicyEntity.PasswordHash))]
    [InlineData(nameof(PolicyEntity.SecurityStamp))]
    [InlineData(nameof(PolicyEntity.ClientSecret))]
    [InlineData(nameof(PolicyEntity.NotFilterable))]
    [InlineData(nameof(PolicyEntity.EncryptedValue))]
    [InlineData(nameof(PolicyEntity.HiddenValue))]
    public void Filter_BySensitiveOrDeniedProperty_ShouldBeRejected(string propertyName)
    {
        var request = new FilterRequest { Criterias = [new FilterCriteria { FilterBy = propertyName, Type = FilterType.EqualTo, Value = "x" }] };

        var act = () => request.BuildFilterExpression<PolicyEntity>();

        act.Should().Throw<MilvaUserFriendlyException>();
    }

    [Fact]
    public void Filter_BySensitiveNameExplicitlyMarkedFilterable_ShouldBeAllowed()
    {
        var request = new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(PolicyEntity.Token), Type = FilterType.EqualTo, Value = "a" }] };

        var expression = request.BuildFilterExpression<PolicyEntity>();

        Entities().AsQueryable().Where(expression).Should().ContainSingle().Which.Id.Should().Be(1);
    }

    [Theory]
    [InlineData(nameof(PolicyEntity.PasswordHash))]
    [InlineData("NotExists")]
    public void Sort_BySensitiveOrUnknownProperty_ShouldBeRejected(string propertyName)
    {
        var act = () => new SortRequest { SortBy = propertyName }.BuildPropertySelectorExpression<PolicyEntity>();

        act.Should().Throw<MilvaUserFriendlyException>();
    }

    [Fact]
    public void Sort_ByNotFilterableProperty_ShouldBeAllowed()
    {
        new SortRequest { SortBy = nameof(PolicyEntity.NotFilterable) }.BuildPropertySelectorExpression<PolicyEntity>().Should().NotBeNull();
    }

    [Theory]
    [InlineData(nameof(PolicyEntity.PasswordHash))]
    [InlineData(nameof(PolicyEntity.NotAggregatable))]
    public async Task Aggregate_BySensitiveOrDeniedProperty_ShouldBeRejected(string propertyName)
    {
        var criteria = new AggregationCriteria { AggregateBy = propertyName, Type = AggregationType.Count };

        var act = () => criteria.ApplyAggregationAsync(Entities().AsQueryable(), runAsync: false, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<MilvaUserFriendlyException>();
    }

    [Fact]
    public void InFilter_WithJsonArray_ShouldBeApplied()
    {
        var value = JsonSerializer.SerializeToElement(new[] { 1, 3 });
        var request = new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(PolicyEntity.Id), Type = FilterType.In, Value = value }] };

        var result = Entities().AsQueryable().Where(request.BuildFilterExpression<PolicyEntity>()).Select(e => e.Id).ToList();

        result.Should().BeEquivalentTo([1, 3]);
    }

    [Fact]
    public void NotInFilter_WithList_ShouldBeApplied()
    {
        var request = new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(PolicyEntity.Name), Type = FilterType.NotIn, Value = new List<string> { "a" } }] };

        var result = Entities().AsQueryable().Where(request.BuildFilterExpression<PolicyEntity>()).Select(e => e.Id).ToList();

        result.Should().BeEquivalentTo([2, 3]);
    }

    [Fact]
    public void InFilter_WithoutList_ShouldBeRejected()
    {
        var request = new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(PolicyEntity.Id), Type = FilterType.In, Value = 1 }] };

        var act = () => request.BuildFilterExpression<PolicyEntity>();

        act.Should().Throw<MilvaUserFriendlyException>();
    }

    [Fact]
    public void RowCount_AboveMaximum_ShouldBeCapped()
    {
        new ListRequest { RowCount = int.MaxValue }.RowCount.Should().Be(RequestQueryOptions.MaxRowCount);
        new CursorListRequest { RowCount = int.MaxValue }.RowCount.Should().Be(RequestQueryOptions.MaxRowCount);
        new ListRequest { RowCount = 10 }.RowCount.Should().Be(10);
    }

    #region Setup

    public class PolicyEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string PasswordHash { get; set; }

        public string SecurityStamp { get; set; }

        public string ClientSecret { get; set; }

        [Filterable(true)]
        public string Token { get; set; }

        [Filterable(false)]
        public string NotFilterable { get; set; }

        [Encrypted]
        public string EncryptedValue { get; set; }

        [HideByRole("Admin")]
        public string HiddenValue { get; set; }

        [Aggregatable(false)]
        public int NotAggregatable { get; set; }
    }

    internal static List<PolicyEntity> Entities() =>
    [
        new() { Id = 1, Name = "a", Token = "a" },
        new() { Id = 2, Name = "b", Token = "b" },
        new() { Id = 3, Name = "c", Token = "c" },
    ];

    #endregion
}

/// <summary>
/// Changes global request options, so it must not run in parallel with other tests.
/// </summary>
[CollectionDefinition(nameof(RequestQueryOptionsCollection), DisableParallelization = true)]
public class RequestQueryOptionsCollection
{
}

[Collection(nameof(RequestQueryOptionsCollection))]
[Trait("Rest Components Unit Tests", "Milvasoft.Components.Rest project unit tests.")]
public class RequestQueryOptionsStrictModeTests
{
    [Fact]
    public void StrictMode_ShouldOnlyAllowExplicitlyMarkedProperties()
    {
        RequestQueryOptions.RequireExplicitOptIn = true;

        try
        {
            var act = () => new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RequestPropertyPolicyTests.PolicyEntity.Name), Type = FilterType.EqualTo, Value = "a" }] }.BuildFilterExpression<RequestPropertyPolicyTests.PolicyEntity>();

            act.Should().Throw<MilvaUserFriendlyException>();

            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RequestPropertyPolicyTests.PolicyEntity.Token), Type = FilterType.EqualTo, Value = "a" }] }
                .BuildFilterExpression<RequestPropertyPolicyTests.PolicyEntity>().Should().NotBeNull();
        }
        finally
        {
            RequestQueryOptions.RequireExplicitOptIn = false;
        }
    }
}
