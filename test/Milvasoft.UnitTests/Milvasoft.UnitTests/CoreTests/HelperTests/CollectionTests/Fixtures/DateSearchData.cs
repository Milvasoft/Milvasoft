using Milvasoft.UnitTests.CoreTests.HelperTests.CollectionTests.Fixtures;
using System.Collections;
using System.Linq.Expressions;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.StringTests.Fixtures;

internal class DateSearchValidDataFixture : IEnumerable<object[]>
{
    /// <summary>
    /// input,  property selector expression, start date, end date, expected result, expected result not contains item 
    /// </summary>
    /// <returns></returns>
    public IEnumerator<object[]> GetEnumerator()
    {
        var validList = new List<DateSearchTestModel>
        {
            new()
            {
                Date = DateTime.Now.AddYears(1),
            },
            new()
            {
                Date = DateTime.Now,
            },
            new()
            {
                Date = DateTime.Now.AddYears(-1),
            },
            new()
            {
                Date = DateTime.Now.AddYears(-2),
            },
            new()
            {
                Date = DateTime.Now.AddYears(-3),
            },
        };

        Expression<Func<DateSearchTestModel, DateTime?>> propertySelectorExpression = i => i.Date;

        yield return new object[] { validList, propertySelectorExpression, null, null, validList };
        yield return new object[] { validList, propertySelectorExpression, DateTime.Now, null, validList.Where(i => i.Date >= DateTime.Now).ToList(), };
        yield return new object[] { validList, propertySelectorExpression, null, DateTime.Now, validList.Where(i => i.Date <= DateTime.Now).ToList() };
        yield return new object[] { validList, propertySelectorExpression, DateTime.Now.AddYears(-3), DateTime.Now, validList.Where(i => i.Date >= DateTime.Now.AddYears(-3) && i.Date <= DateTime.Now).ToList() };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal class DateSearchInvalidDataFixture : IEnumerable<object[]>
{
    /// <summary>
    /// input,  property selector expression, start date, end date, expected result
    /// </summary>
    /// <returns></returns>
    public IEnumerator<object[]> GetEnumerator()
    {
        List<DateSearchTestModel> nullList = null;
        List<DateSearchTestModel> emptyList = [];
        var validList = new List<DateSearchTestModel>
        {
            new()
            {
                Date = DateTime.Now.AddYears(1),
            },
            new()
            {
                Date = DateTime.Now,
            },
            new()
            {
                Date = DateTime.Now.AddYears(-1),
            },
            new()
            {
                Date = DateTime.Now.AddYears(-2),
            },
            new()
            {
                Date = DateTime.Now.AddYears(-3),
            },
        };

        Expression<Func<DateSearchTestModel, DateTime?>> propertySelectorExpression = i => i.Date;
        Expression<Func<DateSearchTestModel, DateTime?>> nullPropertySelectorExpression = null;

        yield return new object[] { nullList, propertySelectorExpression, null, null, nullList };
        yield return new object[] { emptyList, propertySelectorExpression, null, null, emptyList };
        yield return new object[] { validList, nullPropertySelectorExpression, null, null, validList };
        yield return new object[] { validList, nullPropertySelectorExpression, DateTime.Now, null, validList };
        yield return new object[] { validList, propertySelectorExpression, null, null, validList };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}