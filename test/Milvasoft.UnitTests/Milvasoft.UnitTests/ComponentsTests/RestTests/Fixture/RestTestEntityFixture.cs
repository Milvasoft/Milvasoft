using Milvasoft.Core.EntityBases.Abstract;
using System.ComponentModel.DataAnnotations;

namespace Milvasoft.UnitTests.ComponentsTests.RestTests.Fixture;

public class RestTestEntityFixture : IMilvaEntity
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Count { get; set; }
    public RestTestEnumFixture Number { get; set; }
    public bool? IsActive { get; set; }
    public DateTime InsertDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public List<RestChildrenTestEntityFixture> Childrens { get; set; }
    public RestChildrenTestEntityFixture Children { get; set; }

    public object GetUniqueIdentifier() => Id;
}

public class RestChildrenTestEntityFixture
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
}
