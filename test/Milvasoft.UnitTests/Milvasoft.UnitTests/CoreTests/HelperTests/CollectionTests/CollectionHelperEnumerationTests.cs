using FluentAssertions;
using Milvasoft.Core.Helpers;
using System.Collections;

namespace Milvasoft.UnitTests.CoreTests.HelperTests.CollectionTests;

[Trait("Core Unit Tests", "Milvasoft.Core project unit tests.")]
public class CollectionHelperEnumerationTests
{
    public class NullableDateModel
    {
        public string Name { get; set; }
        public DateTime? Date { get; set; }
    }

    private sealed class TrackingEnumerable<T>(params T[] items) : IEnumerable<T>
    {
        public int GetEnumeratorCount { get; private set; }
        public int DisposeCount { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            GetEnumeratorCount++;
            return new TrackingEnumerator(this, items);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private sealed class TrackingEnumerator(TrackingEnumerable<T> owner, T[] items) : IEnumerator<T>
        {
            private int _index = -1;

            public T Current => items[_index];
            object IEnumerator.Current => Current;
            public bool MoveNext() => ++_index < items.Length;
            public void Reset() => _index = -1;
            public void Dispose() => owner.DisposeCount++;
        }
    }

    [Fact]
    public void IsNullOrEmpty_WithLazySequence_ShouldDisposeEnumerator()
    {
        // Arrange
        var source = new TrackingEnumerable<int>(1, 2, 3);

        // Act
        var result = source.IsNullOrEmpty();

        // Assert
        result.Should().BeFalse();
        source.DisposeCount.Should().Be(source.GetEnumeratorCount);
    }

    [Fact]
    public void IsNullOrEmpty_WithQueryable_ShouldUseAnyQuery()
    {
        // Arrange
        var empty = new TrackingEnumerable<int>().AsQueryable();
        var nonEmpty = new TrackingEnumerable<int>(1).AsQueryable();

        // Act & Assert
        empty.IsNullOrEmpty().Should().BeTrue();
        nonEmpty.IsNullOrEmpty().Should().BeFalse();
    }

    [Fact]
    public void OrderByProperty_ShouldNotExecuteSourceQuery()
    {
        // Arrange
        var source = new TrackingEnumerable<NullableDateModel>(new() { Name = "b" }, new() { Name = "a" });
        var queryable = source.AsQueryable();

        // Act
        var ordered = queryable.OrderByProperty(nameof(NullableDateModel.Name));
        var orderedDescending = queryable.OrderByPropertyDescending(nameof(NullableDateModel.Name));

        // Assert
        source.GetEnumeratorCount.Should().Be(0);
        ordered.Select(i => i.Name).Should().Equal("a", "b");
        orderedDescending.Select(i => i.Name).Should().Equal("b", "a");
    }

    [Fact]
    public void ApplyDateSearch_WithNullDates_ShouldExcludeThemInsteadOfThrowing()
    {
        // Arrange
        var date = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        List<NullableDateModel> source =
        [
            new() { Name = "null", Date = null },
            new() { Name = "in", Date = date },
            new() { Name = "out", Date = date.AddDays(10) },
        ];

        // Act
        var act = () => source.ApplyDateSearch(i => i.Date, date.AddDays(-1), date.AddDays(1)).ToList();

        // Assert
        act.Should().NotThrow().Which.Select(i => i.Name).Should().Equal("in");
    }
}
