using ExpressionBuilder.Common;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;

internal class ValidListSourceForBuildFilterExpressionMethodData : TheoryData<IQueryable<RestTestEntityFixture>, FilterRequest, List<int>>
{
    /// <summary>
    /// source , filter request, expected filtered result id list
    /// </summary>
    /// <returns></returns>
    public ValidListSourceForBuildFilterExpressionMethodData()
    {
        IQueryable<RestTestEntityFixture> validQueryable = new List<RestTestEntityFixture>
        {
            new() {
                Id = 1,
                Name = "John",
                Count = 1,
                Price = 10M,
                Number = RestTestEnumFixture.Two,
                IsActive = true,
                InsertDate = DateTime.Now,
                UpdateDate = DateTime.Now.AddDays(1),
                Children = new()
                {
                    Name = "Jack"
                },
                Childrens = []
            },
            new() {
                Id = 2,
                Name = "Elise",
                Count = 10,
                Price = 15.5M,
                Number = RestTestEnumFixture.One,
                IsActive = false,
                InsertDate = DateTime.Now.AddDays(4),
                UpdateDate = DateTime.Now.AddDays(3),
                Children = new()
                {
                    Name = "John"
                },
                Childrens =
                [
                    new()
                    {
                        Name = "John"
                    },
                    new()
                    {
                        Name = "Jack"
                    }
                ]
            },
            new() {
                Id = 3,
                Name = "Jack",
                Count = 90,
                Price = 0.50M,
                Number = RestTestEnumFixture.Zero,
                IsActive = false,
                InsertDate = DateTime.Now,
                UpdateDate = null,
                Childrens = []
            },
            new() {
                Id = 4,
                Name = "Mary",
                Count = 101,
                Price = 4M,
                Number = RestTestEnumFixture.One,
                IsActive = true,
                InsertDate = DateTime.Now,
                UpdateDate = null,
                Childrens =
                [
                    new()
                    {

                        Name = "Elise"
                    }
                ]
            },
            new() {
                Id = 5,
                Name = null,
                Count = 999,
                Price = 999M,
                Number = RestTestEnumFixture.Zero,
                IsActive = null,
                InsertDate = DateTime.Now.AddMonths(-5),
                UpdateDate = null,
                Childrens = []
            },
        }.AsQueryable();

        //1
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNull }] },
            [5]
        );

        //2
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.Contains, Value = "J" }] },
            [1, 3]
        );

        //3
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.Contains, Value = "y" }] },
            [4]
        );

        //4
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.EqualTo, Value = "Mary" }] },
            [4]
        );

        //5
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Count), Type = FilterType.GreaterThanOrEqualTo, Value = 90 }] },
            [3, 4, 5]
        );

        //6
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Count), Type = FilterType.Between, Value = 90, OtherValue = 100 }] },
            [3]
        );

        //7
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.IsActive), Type = FilterType.EqualTo, Value = true }] },
            [1, 4]
        );

        //8
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.IsActive), Type = FilterType.NotEqualTo, Value = true }] },
            [2, 3]
        );

        //9
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.InsertDate), Type = FilterType.Between, Value = DateTime.Now.AddDays(-1), OtherValue = DateTime.Now.AddDays(2) }] },
            [1, 3, 4]
        );

        //10
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.UpdateDate), Type = FilterType.Between, Value = DateTime.Now.AddDays(-1), OtherValue = DateTime.Now.AddDays(2) }] },
            [1]
        );

        //11
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.UpdateDate), Type = FilterType.DateEqualTo, Value = null }] },
            [3, 4, 5]
        );

        //12
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.IsActive), Type = FilterType.EqualTo, Value = null }] },
            [5]
        );

        //13
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNullOrWhiteSpace }] },
            [5]
        );

        //14
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Number), Type = FilterType.EqualTo, Value = RestTestEnumFixture.Two }] },
            [1]
        );

        //15
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = $"{nameof(RestTestEntityFixture.Children)}.Name", Type = FilterType.EqualTo, Value = "Jack" }] },
            [1]
        );

        //16
        Add(
            validQueryable,
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = $"{nameof(RestTestEntityFixture.Childrens)}[Name]", Type = FilterType.EqualTo, Value = "John" }] },
            [2]
        );

        //17
        Add(
            validQueryable,
            new FilterRequest
            {
                Criterias =
                [
                    new FilterCriteria
                    {
                        FilterBy = nameof(RestTestEntityFixture.Count),
                        Type = FilterType.GreaterThanOrEqualTo,
                        Value = 100
                    },
                    new FilterCriteria
                    {
                        FilterBy = nameof(RestTestEntityFixture.Count),
                        Type = FilterType.LessThanOrEqualTo,
                        Value = 900
                    },
                    new FilterCriteria
                    {
                        FilterBy = nameof(RestTestEntityFixture.Count),
                        Type = FilterType.LessThanOrEqualTo,
                        Value = 900
                    }
                ],
                MergeType = Connector.And
            },
            [4]
        );

        //18
        Add(
            validQueryable,
            new FilterRequest
            {
                Criterias =
                [
                    new FilterCriteria
                    {
                        FilterBy = nameof(RestTestEntityFixture.Name),
                        Type = FilterType.Contains,
                        Value = "j"
                    },
                    new FilterCriteria
                    {
                        FilterBy = nameof(RestTestEntityFixture.Number),
                        Type = FilterType.EqualTo,
                        Value = RestTestEnumFixture.One
                    }
                ],
                MergeType = Connector.Or
            },
            [2, 4]
        );
    }
}

internal class InvalidListSourceForBuildFilterExpressionMethodData : TheoryData<FilterRequest>
{
    /// <summary>
    /// filter request
    /// </summary>
    /// <returns></returns>
    public InvalidListSourceForBuildFilterExpressionMethodData()
    {
        Add(
            new FilterRequest
            {
                Criterias =
                [
                    new()
                    {
                        FilterBy = null
                    },
                    new()
                    {
                        FilterBy = ""
                    },
                    new()
                    {
                        FilterBy = " "
                    },
                    new()
                    {
                        FilterBy = "NotExistsPropertyName"
                    },
                    new()
                    {
                        FilterBy = nameof(RestTestEntityFixture.Name),
                        Type = FilterType.Between
                    },
                    new()
                    {
                        FilterBy = nameof(RestTestEntityFixture.Count),
                        Type = FilterType.Contains
                    },
                    new()
                    {
                        FilterBy = nameof(RestTestEntityFixture.IsActive),
                        Type = FilterType.GreaterThan
                    },
                    new()
                    {
                        FilterBy = nameof(RestTestEntityFixture.InsertDate),
                        Type = FilterType.EndsWith
                    },
                    new()
                    {
                        FilterBy = nameof(RestTestEntityFixture.InsertDate),
                        Type = FilterType.IsNullOrWhiteSpace
                    }
                ]
            }
        );
    }
}

internal class ValidListSourceButFilterValuesNotValidForBuildFilterExpressionMethodData : TheoryData<FilterRequest>
{
    /// <summary>
    /// filter request
    /// </summary>
    /// <returns></returns>
    public ValidListSourceButFilterValuesNotValidForBuildFilterExpressionMethodData()
    {
        Add(
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Count), Type = FilterType.EqualTo, Value = null }] }
        );

        Add(
            new FilterRequest { Criterias = [new FilterCriteria { FilterBy = nameof(RestTestEntityFixture.Count), Type = FilterType.Between, Value = 90, OtherValue = null }] }
        );
    }
}
