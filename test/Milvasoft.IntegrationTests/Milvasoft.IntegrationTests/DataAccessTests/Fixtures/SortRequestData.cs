using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;
using Milvasoft.IntegrationTests.Client.Fixtures.EnumFixtures;
using System.Collections;
using System.Linq.Expressions;

namespace Milvasoft.IntegrationTests.DataAccessTests.Fixtures;

internal class ValidAscendingSortRequestForBuildPropertySelectorExpressionMethodData : IEnumerable<object[]>
{
    /// <summary>
    /// source , filter request, expected filtered result id list
    /// </summary>
    /// <returns></returns>
    public IEnumerator<object[]> GetEnumerator()
    {
        List<RestTestEntityFixture> validList =
        [
            new() {
                Id = 1,
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two,
                IsActive = true,
                InsertDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow.AddDays(1),
            },
            new() {
                Id = 2,
                Name = "Elise",
                Count = 10,
                Price = 15.5M,
                Number = RestTestEnumFixture.One,
                IsActive = false,
                InsertDate = DateTime.UtcNow.AddDays(4),
                UpdateDate = DateTime.UtcNow.AddDays(3),
            },
            new() {
                Id = 3,
                Name = "Jack",
                Count = 90,
                Price = 0.50M,
                Number = RestTestEnumFixture.Zero,
                IsActive = false,
                InsertDate = DateTime.UtcNow,
                UpdateDate = null
            },
            new() {
                Id = 4,
                Name = "Mary",
                Count = 101,
                Price = 4M,
                Number = RestTestEnumFixture.One,
                IsActive = true,
                InsertDate = DateTime.UtcNow,
                UpdateDate = null
            },
            new() {
                Id = 5,
                Name = null,
                Count = 999,
                Price = 999M,
                Number = RestTestEnumFixture.Zero,
                IsActive = null,
                InsertDate = DateTime.UtcNow.AddMonths(-5),
                UpdateDate = null
            },
        ];

        Expression<Func<RestTestEntityFixture, object>> nameSelector = i => i.Name;
        Expression<Func<RestTestEntityFixture, object>> numberSelector = i => i.Number;
        Expression<Func<RestTestEntityFixture, object>> countSelector = i => i.Count;
        Expression<Func<RestTestEntityFixture, object>> priceSelector = i => i.Price;
        Expression<Func<RestTestEntityFixture, object>> isActiveSelector = i => i.IsActive;
        Expression<Func<RestTestEntityFixture, object>> insertDateSelector = i => i.InsertDate;
        Expression<Func<RestTestEntityFixture, object>> updateDateSelector = i => i.UpdateDate;

        //1
        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Number), Type = SortType.Asc },
            numberSelector
        };

        //2
        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Name), Type = SortType.Asc },
            nameSelector
        };

        //3
        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Count), Type = SortType.Asc },
            countSelector
        };

        //4
        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Price), Type = SortType.Asc },
            priceSelector
        };

        //5
        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.IsActive), Type = SortType.Asc },
            isActiveSelector
        };

        //6
        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.InsertDate), Type = SortType.Asc },
            insertDateSelector
        };

        //7
        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.UpdateDate), Type = SortType.Asc },
            updateDateSelector
        };
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal class ValidDescendingSortRequestForBuildPropertySelectorExpressionMethodData : IEnumerable<object[]>
{
    /// <summary>
    /// source , filter request, expected filtered result id list
    /// </summary>
    /// <returns></returns>
    public IEnumerator<object[]> GetEnumerator()
    {
        List<RestTestEntityFixture> validList =
        [
            new() {
                Id = 1,
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two,
                IsActive = true,
                InsertDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow.AddDays(1),
            },
            new() {
                Id = 2,
                Name = "Elise",
                Count = 10,
                Price = 15.5M,
                Number = RestTestEnumFixture.One,
                IsActive = false,
                InsertDate = DateTime.UtcNow.AddDays(4),
                UpdateDate = DateTime.UtcNow.AddDays(3),
            },
            new() {
                Id = 3,
                Name = "Jack",
                Count = 90,
                Price = 0.50M,
                Number = RestTestEnumFixture.Zero,
                IsActive = false,
                InsertDate = DateTime.UtcNow,
                UpdateDate = null
            },
            new() {
                Id = 4,
                Name = "Mary",
                Count = 101,
                Price = 4M,
                Number = RestTestEnumFixture.One,
                IsActive = true,
                InsertDate = DateTime.UtcNow,
                UpdateDate = null
            },
            new() {
                Id = 5,
                Name = null,
                Count = 999,
                Price = 999M,
                Number = RestTestEnumFixture.Zero,
                IsActive = null,
                InsertDate = DateTime.UtcNow.AddMonths(-5),
                UpdateDate = null
            },
        ];

        Expression<Func<RestTestEntityFixture, object>> nameSelector = i => i.Name;
        Expression<Func<RestTestEntityFixture, object>> numberSelector = i => i.Number;
        Expression<Func<RestTestEntityFixture, object>> countSelector = i => i.Count;
        Expression<Func<RestTestEntityFixture, object>> priceSelector = i => i.Price;
        Expression<Func<RestTestEntityFixture, object>> isActiveSelector = i => i.IsActive;
        Expression<Func<RestTestEntityFixture, object>> insertDateSelector = i => i.InsertDate;
        Expression<Func<RestTestEntityFixture, object>> updateDateSelector = i => i.UpdateDate;

        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Number), Type = SortType.Desc },
            numberSelector
        };

        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Name), Type = SortType.Desc },
            nameSelector
        };

        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Count), Type = SortType.Desc },
            countSelector
        };

        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.Price), Type = SortType.Desc },
            priceSelector
        };

        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.IsActive), Type = SortType.Desc },
            isActiveSelector
        };

        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.InsertDate), Type = SortType.Desc },
            insertDateSelector
        };

        yield return new object[]
        {
            validList,
            new SortRequest{   SortBy = nameof(RestTestEntityFixture.UpdateDate), Type = SortType.Desc },
            updateDateSelector
        };
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}