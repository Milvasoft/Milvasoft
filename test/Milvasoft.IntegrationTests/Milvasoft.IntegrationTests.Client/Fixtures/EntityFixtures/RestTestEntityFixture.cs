﻿using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.IntegrationTests.Client.Fixtures.EnumFixtures;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milvasoft.IntegrationTests.Client.Fixtures.EntityFixtures;

public class RestTestEntityFixture : IAuditable<int>
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
    public DateTime? CreationDate { get; set; }
    public string CreatorUserName { get; set; }
    public DateTime? LastModificationDate { get; set; }
    public string LastModifierUserName { get; set; }

    public virtual List<RestChildrenTestEntityFixture> Childrens { get; set; }

    public object GetUniqueIdentifier() => Id;
}

public class RestChildrenTestEntityFixture
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }

    [ForeignKey(nameof(Parent))]
    public int? ParentId { get; set; }
    public virtual RestTestEntityFixture Parent { get; set; }
}

