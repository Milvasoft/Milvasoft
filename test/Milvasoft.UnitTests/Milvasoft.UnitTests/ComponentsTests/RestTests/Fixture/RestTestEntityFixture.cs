using System.ComponentModel.DataAnnotations;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;

public class RestTestEntityFixture
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Count { get; set; }
    public RestTestEnumFixture Number { get; set; }
}
