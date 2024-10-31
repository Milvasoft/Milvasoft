using ExpressionBuilder.Common;
using Milvasoft.Components.Rest.Enums;
using Milvasoft.Components.Rest.Request;
using System.Collections;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;

internal class ValidListSourceForBuildFilterExpressionMethodData : IEnumerable<object[]>
{
    /// <summary>
    /// source , filter request, expected filtered result id list
    /// </summary>
    /// <returns></returns>
    public IEnumerator<object[]> GetEnumerator()
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
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNull }]},
            new List<int> { 5 }
        };

        //2
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.Contains, Value = "J"}]},
            new List<int> { 1, 3 }
        };

        //3
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.Contains, Value = "j"}]},
            new List<int> { 1, 3 }
        };

        //4
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.EqualTo, Value = "Mary"}]},
            new List<int> { 4 }
        };

        //5
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Count), Type = FilterType.GreaterThanOrEqualTo, Value = 90}]},
            new List<int> { 3, 4, 5 }
        };

        //6
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Count), Type = FilterType.Between, Value = 90, OtherValue = 100 }]},
            new List<int> { 3 }
        };

        //7
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.IsActive), Type = FilterType.EqualTo, Value = true }]},
            new List<int> { 1 , 4 }
        };

        //8
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.IsActive), Type = FilterType.NotEqualTo, Value = true }]},
            new List<int> { 2, 3 }
        };

        //9
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.InsertDate), Type = FilterType.Between, Value = DateTime.Now.AddDays(-1), OtherValue = DateTime.Now.AddDays(2)}]},
            new List<int> { 1, 3, 4 }
        };

        //10
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.UpdateDate), Type = FilterType.Between, Value = DateTime.Now.AddDays(-1), OtherValue = DateTime.Now.AddDays(2)}]},
            new List<int> { 1 }
        };

        //11
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.UpdateDate), Type = FilterType.DateEqualTo, Value = null }]},
            new List<int> { 3 , 4 , 5}
        };

        //12
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{  Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.IsActive), Type = FilterType.EqualTo, Value = null }]},
            new List<int> { 5 }
        };

        //13
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{  Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Name), Type = FilterType.IsNullOrWhiteSpace }]},
            new List<int> { 5 }
        };

        //14
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{  Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Number), Type = FilterType.EqualTo, Value = RestTestEnumFixture.Two }]},
            new List<int> { 1 }
        };

        //15
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{  Criterias = [ new FilterCriteria{ FilterBy = $"{nameof(RestTestEntityFixture.Children)}.Name", Type = FilterType.EqualTo, Value = "Jack" }]},
            new List<int> { 1 }
        };

        //16
        yield return new object[]
        {
            validQueryable,
            new FilterRequest{  Criterias = [ new FilterCriteria{ FilterBy = $"{nameof(RestTestEntityFixture.Childrens)}[Name]", Type = FilterType.EqualTo, Value = "John" }]},
            new List<int> { 2 }
        };

        //17
        yield return new object[]
        {
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
                    }
                ],
                MergeType = Connector.And
            },
            new List<int> { 4 }
        };

        //18
        yield return new object[]
        {
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
            new List<int> { 1,2,3,4 }
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal class InvalidListSourceForBuildFilterExpressionMethodData : IEnumerable<object[]>
{
    /// <summary>
    /// filter request
    /// </summary>
    /// <returns></returns>
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
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
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal class ValidListSourceButFilterValuesNotValidForBuildFilterExpressionMethodData : IEnumerable<object[]>
{
    /// <summary>
    /// filter request
    /// </summary>
    /// <returns></returns>
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Count), Type = FilterType.EqualTo, Value = null }]}
        };

        yield return new object[]
        {
            new FilterRequest{ Criterias = [ new FilterCriteria{ FilterBy = nameof(RestTestEntityFixture.Count), Type = FilterType.Between, Value = 90, OtherValue = null }]}
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
