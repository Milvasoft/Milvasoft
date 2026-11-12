using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.EntityBases.Concrete.Auditing;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvasoft.DataAccess.EfCore.DbContextBase;
using Milvasoft.DataAccess.EfCore.Utils.Enums;
using Milvasoft.Helpers.DataAccess.EfCore.Concrete;
using Npgsql;

namespace Milvasoft.IntegrationTests.DataAccessTests.EfCoreTests;

/// <summary>
/// Soft deletion and auditing behavior of <see cref="MilvaDbContext"/> against PostgreSQL.
/// Every test uses its own database created from <see cref="SoftDeletionTestDbContext"/>.
/// </summary>
[Collection(nameof(UtcTrueDatabaseTestCollection))]
[Trait("MilvaDbContext Soft Deletion Integration Tests", "Soft deletion and auditing behavior of MilvaDbContext.")]
public class SoftDeletionAndAuditingTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    private readonly string _connectionString = new NpgsqlConnectionStringBuilder(factory.GetConnectionString())
    {
        Database = $"sd_{Guid.NewGuid():N}"
    }.ConnectionString;

    private string _currentUserName = "user1";

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        GC.SuppressFinalize(this);
    }

    #region Soft deletion

    [Theory]
    [InlineData(SaveMethod.Sync)]
    [InlineData(SaveMethod.Async)]
    [InlineData(SaveMethod.SyncWithAcceptAllChanges)]
    [InlineData(SaveMethod.AsyncWithAcceptAllChanges)]
    public async Task SoftDeletion_RemovingTrackedPrincipalWithLoadedGraph_ShouldSoftDeleteOnlyCascadeDependentsAndKeepEverythingElseIntact(SaveMethod saveMethod)
    {
        // Arrange
        var ids = await SeedGraphAsync();
        var ct = TestContext.Current.CancellationToken;

        await using (var context = CreateContext())
        {
            var blog = await context.Blogs.Include(b => b.Author)
                                          .Include(b => b.Posts).ThenInclude(p => p.Tags)
                                          .Include(b => b.Posts).ThenInclude(p => p.Comments)
                                          .Include(b => b.Notes)
                                          .Include(b => b.Translations)
                                          .Include(b => b.Attachments)
                                          .Include(b => b.Links)
                                          .SingleAsync(b => b.Id == ids.BlogId, ct);

            // Act
            context.Blogs.Remove(blog);
            await SaveAsync(context, saveMethod);

            // In memory: the removal side effects of EF must not leak to the remaining graph.
            context.Entry(blog).State.Should().Be(EntityState.Unchanged);
            blog.Notes.Should().ContainSingle();
            blog.Notes[0].BlogId.Should().Be(ids.BlogId);
        }

        // Assert
        await using var assertContext = CreateContext();

        var blogInDb = await assertContext.Blogs.AsNoTracking().SingleAsync(b => b.Id == ids.BlogId, ct);
        blogInDb.IsDeleted.Should().BeTrue();
        blogInDb.DeletionDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        blogInDb.DeleterUserName.Should().Be("user1");
        blogInDb.Name.Should().Be("blog");
        blogInDb.AuthorId.Should().Be(ids.AuthorId);
        blogInDb.CreationDate.Should().NotBeNull();
        blogInDb.CreatorUserName.Should().Be("seeder");
        blogInDb.LastModificationDate.Should().BeNull();
        blogInDb.LastModifierUserName.Should().BeNull();
        blogInDb.Address.Should().NotBeNull();
        blogInDb.Address.City.Should().Be("Ankara");

        // Principal of the deleted entity is never soft deleted.
        var authorInDb = await assertContext.Authors.AsNoTracking().SingleAsync(ct);
        authorInDb.IsDeleted.Should().BeFalse();

        // Cascade dependents (and their cascade dependents) are soft deleted.
        var postsInDb = await assertContext.Posts.AsNoTracking().ToListAsync(ct);
        postsInDb.Should().HaveCount(2).And.OnlyContain(p => p.IsDeleted && p.DeletionDate != null && p.Title.StartsWith("post"));
        var commentInDb = await assertContext.Comments.AsNoTracking().SingleAsync(ct);
        commentInDb.IsDeleted.Should().BeTrue();
        commentInDb.PostId.Should().Be(ids.FirstPostId);

        // Entities reachable through skip navigations (many to many) are never soft deleted and join rows are kept.
        var tagsInDb = await assertContext.Tags.AsNoTracking().Include(t => t.Posts).ToListAsync(ct);
        tagsInDb.Should().HaveCount(2).And.OnlyContain(t => !t.IsDeleted && t.Posts.Count == 1);

        // Optional dependent is kept with its foreign key (EF sets it to null when the principal is removed).
        var noteInDb = await assertContext.Notes.AsNoTracking().SingleAsync(ct);
        noteInDb.IsDeleted.Should().BeFalse();
        noteInDb.BlogId.Should().Be(ids.BlogId);

        // Not soft deletable cascade dependent is kept, because the soft deleted row it depends on still exists.
        var translationInDb = await assertContext.Translations.AsNoTracking().SingleAsync(ct);
        translationInDb.BlogId.Should().Be(ids.BlogId);
        translationInDb.Value.Should().Be("translation");

        await AssertCascadeOnDeleteChildrenAsync(assertContext, ids.BlogId);
    }

    [Fact]
    public async Task SoftDeletion_DeletingThroughRepositoryWithCascadeOnDeleteIncludes_ShouldSoftDeleteMarkedChildrenAndKeepForeignKeys()
    {
        // Arrange
        var ids = await SeedGraphAsync();
        var ct = TestContext.Current.CancellationToken;

        await using (var context = CreateContext())
        {
            var repository = new SdBlogRepository(context);

            // Includes [CascadeOnDelete] marked navigations (Attachments, Links).
            var blog = await repository.GetForDeleteAsync(ids.BlogId, cancellationToken: ct);

            blog.Attachments.Should().ContainSingle();
            blog.Links.Should().ContainSingle();

            // Act
            await repository.DeleteAsync(blog, ct);
        }

        // Assert
        await using var assertContext = CreateContext();

        (await assertContext.Blogs.AsNoTracking().SingleAsync(ct)).IsDeleted.Should().BeTrue();

        // Optional child without the attribute: kept with its foreign key.
        var noteInDb = await assertContext.Notes.AsNoTracking().SingleAsync(ct);
        noteInDb.IsDeleted.Should().BeFalse();
        noteInDb.BlogId.Should().Be(ids.BlogId);

        // Required cascade children (not included) are not tracked, so they are not touched.
        (await assertContext.Posts.AsNoTracking().ToListAsync(ct)).Should().HaveCount(2).And.OnlyContain(p => !p.IsDeleted);

        await AssertCascadeOnDeleteChildrenAsync(assertContext, ids.BlogId);
    }

    /// <summary>
    /// Optional soft deletable [CascadeOnDelete] child is soft deleted with its foreign key unchanged,
    /// optional not soft deletable [CascadeOnDelete] child is hard deleted.
    /// </summary>
    private static async Task AssertCascadeOnDeleteChildrenAsync(SoftDeletionTestDbContext assertContext, int blogId)
    {
        var ct = TestContext.Current.CancellationToken;

        var attachmentInDb = await assertContext.Attachments.AsNoTracking().SingleAsync(ct);
        attachmentInDb.IsDeleted.Should().BeTrue();
        attachmentInDb.DeletionDate.Should().NotBeNull();
        attachmentInDb.DeleterUserName.Should().Be("user1");
        attachmentInDb.BlogId.Should().Be(blogId);
        attachmentInDb.Name.Should().Be("attachment");
        attachmentInDb.LastModificationDate.Should().BeNull();

        (await assertContext.Links.AsNoTracking().CountAsync(ct)).Should().Be(0);
    }

    [Fact]
    public async Task SoftDeletion_RemovingStubEntity_ShouldOnlyWriteSoftDeletionColumns()
    {
        // Arrange
        var ids = await SeedGraphAsync();
        var ct = TestContext.Current.CancellationToken;

        await using (var context = CreateContext())
        {
            // Act: e.g. entity fetched with a projection, other columns have default values.
            context.Blogs.Remove(new SdBlog { Id = ids.BlogId });
            await context.SaveChangesAsync(ct);
        }

        // Assert
        await using var assertContext = CreateContext();

        var blogInDb = await assertContext.Blogs.AsNoTracking().SingleAsync(b => b.Id == ids.BlogId, ct);
        blogInDb.IsDeleted.Should().BeTrue();
        blogInDb.DeletionDate.Should().NotBeNull();
        blogInDb.Name.Should().Be("blog");
        blogInDb.AuthorId.Should().Be(ids.AuthorId);
        blogInDb.CreationDate.Should().NotBeNull();
        blogInDb.CreatorUserName.Should().Be("seeder");
        blogInDb.Address.City.Should().Be("Ankara");
    }

    [Fact]
    public async Task SoftDeletion_RemovingDependentWithLoadedPrincipalAndSkipNavigation_ShouldNotSoftDeletePrincipalOrTags()
    {
        // Arrange
        var ids = await SeedGraphAsync();
        var ct = TestContext.Current.CancellationToken;

        await using (var context = CreateContext())
        {
            var post = await context.Posts.Include(p => p.Blog).ThenInclude(b => b.Author)
                                          .Include(p => p.Tags)
                                          .SingleAsync(p => p.Id == ids.FirstPostId, ct);

            // Act
            context.Posts.Remove(post);
            await context.SaveChangesAsync(ct);
        }

        // Assert
        await using var assertContext = CreateContext();

        (await assertContext.Posts.AsNoTracking().SingleAsync(p => p.Id == ids.FirstPostId, ct)).IsDeleted.Should().BeTrue();
        (await assertContext.Posts.AsNoTracking().SingleAsync(p => p.Id != ids.FirstPostId, ct)).IsDeleted.Should().BeFalse();
        (await assertContext.Blogs.AsNoTracking().SingleAsync(ct)).IsDeleted.Should().BeFalse();
        (await assertContext.Authors.AsNoTracking().SingleAsync(ct)).IsDeleted.Should().BeFalse();
        (await assertContext.Tags.AsNoTracking().Include(t => t.Posts).ToListAsync(ct)).Should().HaveCount(2).And.OnlyContain(t => !t.IsDeleted && t.Posts.Count == 1);
    }

    [Fact]
    public async Task SoftDeletion_WithOnSaveChangesCascadeTiming_ShouldSoftDeleteLoadedCascadeDependents()
    {
        // Arrange
        var ids = await SeedGraphAsync();
        var ct = TestContext.Current.CancellationToken;

        await using (var context = CreateContext())
        {
            context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;

            var blog = await context.Blogs.Include(b => b.Posts).ThenInclude(p => p.Comments)
                                          .Include(b => b.Notes)
                                          .Include(b => b.Translations)
                                          .Include(b => b.Attachments)
                                          .Include(b => b.Links)
                                          .SingleAsync(b => b.Id == ids.BlogId, ct);

            // Act
            context.Blogs.Remove(blog);
            await context.SaveChangesAsync(ct);
        }

        // Assert
        await using var assertContext = CreateContext();

        (await assertContext.Blogs.AsNoTracking().SingleAsync(ct)).IsDeleted.Should().BeTrue();
        (await assertContext.Posts.AsNoTracking().ToListAsync(ct)).Should().HaveCount(2).And.OnlyContain(p => p.IsDeleted);
        (await assertContext.Comments.AsNoTracking().SingleAsync(ct)).IsDeleted.Should().BeTrue();
        var noteInDb = await assertContext.Notes.AsNoTracking().SingleAsync(ct);
        noteInDb.IsDeleted.Should().BeFalse();
        noteInDb.BlogId.Should().Be(ids.BlogId);
        (await assertContext.Translations.AsNoTracking().CountAsync(ct)).Should().Be(1);
        await AssertCascadeOnDeleteChildrenAsync(assertContext, ids.BlogId);
    }

    [Fact]
    public async Task SoftDeletion_WithPassiveState_ShouldHardDeleteGraph()
    {
        // Arrange
        var ids = await SeedGraphAsync();
        var ct = TestContext.Current.CancellationToken;

        await using (var context = CreateContext())
        {
            context.ChangeSoftDeletionState(SoftDeletionState.Passive);

            var post = await context.Posts.Include(p => p.Comments).SingleAsync(p => p.Id == ids.FirstPostId, ct);

            // Act
            context.Posts.Remove(post);
            await context.SaveChangesAsync(ct);
        }

        // Assert
        await using var assertContext = CreateContext();

        (await assertContext.Posts.AsNoTracking().CountAsync(ct)).Should().Be(1);
        (await assertContext.Comments.AsNoTracking().CountAsync(ct)).Should().Be(0);
        (await assertContext.Tags.AsNoTracking().CountAsync(ct)).Should().Be(2);
    }

    #endregion

    #region Auditing

    [Fact]
    public async Task Auditing_ModifyingEntityMoreThanOnce_ShouldWriteModificationAuditValuesOnEveryModification()
    {
        // Arrange
        var ids = await SeedGraphAsync();
        var ct = TestContext.Current.CancellationToken;

        await using var context = CreateContext();

        var blog = await context.Blogs.SingleAsync(b => b.Id == ids.BlogId, ct);

        // Act
        blog.Name = "first update";
        await context.SaveChangesAsync(ct);
        var firstModificationDate = blog.LastModificationDate;

        await Task.Delay(50, ct);
        _currentUserName = "user2";
        blog.Name = "second update";
        await context.SaveChangesAsync(ct);

        // Assert
        await using var assertContext = CreateContext();

        var blogInDb = await assertContext.Blogs.AsNoTracking().SingleAsync(b => b.Id == ids.BlogId, ct);
        firstModificationDate.Should().NotBeNull();
        blogInDb.Name.Should().Be("second update");
        blogInDb.LastModificationDate.Should().BeAfter(firstModificationDate.Value);
        blogInDb.LastModifierUserName.Should().Be("user2");
    }

    [Theory]
    [InlineData(SaveMethod.SyncWithAcceptAllChanges)]
    [InlineData(SaveMethod.AsyncWithAcceptAllChanges)]
    public async Task Auditing_WithAcceptAllChangesOverloads_ShouldAuditAndSoftDelete(SaveMethod saveMethod)
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;

        await using var context = CreateContext();

        var author = new SdAuthor { Name = "author" };

        // Act & Assert
        context.Authors.Add(author);
        await SaveAsync(context, saveMethod);
        author.CreationDate.Should().NotBeNull();
        author.CreatorUserName.Should().Be("user1");

        author.Name = "updated";
        await SaveAsync(context, saveMethod);
        author.LastModificationDate.Should().NotBeNull();
        author.LastModifierUserName.Should().Be("user1");

        context.Authors.Remove(author);
        await SaveAsync(context, saveMethod);

        await using var assertContext = CreateContext();
        var authorInDb = await assertContext.Authors.AsNoTracking().SingleAsync(ct);
        authorInDb.IsDeleted.Should().BeTrue();
        authorInDb.DeletionDate.Should().NotBeNull();
        authorInDb.Name.Should().Be("updated");
    }

    [Fact]
    public async Task Auditing_UpdatingDetachedEntityWithoutCreationValues_ShouldNotOverwriteCreationAuditValues()
    {
        // Arrange
        var ids = await SeedGraphAsync();
        var ct = TestContext.Current.CancellationToken;

        await using (var context = CreateContext())
        {
            // Act: e.g. entity mapped from a request dto.
            context.Blogs.Update(new SdBlog
            {
                Id = ids.BlogId,
                Name = "updated",
                AuthorId = ids.AuthorId,
                Address = new SdAddress { City = "Izmir" }
            });

            await context.SaveChangesAsync(ct);
        }

        // Assert
        await using var assertContext = CreateContext();

        var blogInDb = await assertContext.Blogs.AsNoTracking().SingleAsync(b => b.Id == ids.BlogId, ct);
        blogInDb.Name.Should().Be("updated");
        blogInDb.CreationDate.Should().NotBeNull();
        blogInDb.CreatorUserName.Should().Be("seeder");
        blogInDb.LastModificationDate.Should().NotBeNull();
        blogInDb.LastModifierUserName.Should().Be("user1");
    }

    #endregion

    #region Transactions

    [Fact]
    public async Task ApplyTransactionAsync_WithFailingFunction_ShouldRollbackRunRollbackFunctionAndRethrowOriginalException()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var context = CreateContext();
        var repository = new Milvasoft.DataAccess.EfCore.RepositoryBase.Concrete.ContextRepository<SoftDeletionTestDbContext>(context);
        var asyncRollbackCalled = false;
        var syncRollbackCalled = false;

        // Act
        var asyncAct = () => repository.ApplyTransactionAsync(async () =>
        {
            context.Authors.Add(new SdAuthor { Name = "rolled back" });
            await context.SaveChangesAsync(ct);
            throw new InvalidOperationException("original");
        }, () => { asyncRollbackCalled = true; return Task.CompletedTask; }, cancellationToken: ct);

        var syncAct = () => repository.ApplyTransactionAsync<int>(() => throw new ArgumentException("original"), () => syncRollbackCalled = true, cancellationToken: ct);

        // Assert
        await asyncAct.Should().ThrowAsync<InvalidOperationException>().WithMessage("original");
        await syncAct.Should().ThrowAsync<ArgumentException>().WithMessage("original");
        asyncRollbackCalled.Should().BeTrue();
        syncRollbackCalled.Should().BeTrue();
        context.Database.CurrentTransaction.Should().BeNull();

        await using var assertContext = CreateContext();
        (await assertContext.Authors.CountAsync(ct)).Should().Be(0);
    }

    [Fact]
    public async Task ApplyTransactionAsync_WithSucceedingFunction_ShouldCommitAndReturnResult()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await using var context = CreateContext();
        var repository = new Milvasoft.DataAccess.EfCore.RepositoryBase.Concrete.ContextRepository<SoftDeletionTestDbContext>(context);

        // Act
        var result = await repository.ApplyTransactionAsync(async () =>
        {
            context.Authors.Add(new SdAuthor { Name = "committed" });
            return await context.SaveChangesAsync(ct);
        }, cancellationToken: ct);

        // Assert
        result.Should().Be(1);
        context.Database.CurrentTransaction.Should().BeNull();
        await using var assertContext = CreateContext();
        (await assertContext.Authors.SingleAsync(ct)).Name.Should().Be("committed");
    }

    #endregion

    #region Helpers

    public enum SaveMethod
    {
        Sync,
        Async,
        SyncWithAcceptAllChanges,
        AsyncWithAcceptAllChanges
    }

    private static async Task SaveAsync(SoftDeletionTestDbContext context, SaveMethod saveMethod)
    {
        var ct = TestContext.Current.CancellationToken;

        switch (saveMethod)
        {
            case SaveMethod.Sync:
                context.SaveChanges();
                break;
            case SaveMethod.Async:
                await context.SaveChangesAsync(ct);
                break;
            case SaveMethod.SyncWithAcceptAllChanges:
                context.SaveChanges(acceptAllChangesOnSuccess: true);
                break;
            case SaveMethod.AsyncWithAcceptAllChanges:
                await context.SaveChangesAsync(acceptAllChangesOnSuccess: true, ct);
                break;
        }
    }

    private SoftDeletionTestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SoftDeletionTestDbContext>().UseNpgsql(_connectionString).Options;

        var configuration = new DataAccessConfiguration
        {
            DbContext = new DbContextConfiguration
            {
                UseUtcForDateTime = true,
                DefaultSoftDeletionState = SoftDeletionState.Active,
                GetCurrentUserNameMethod = _ => _currentUserName
            },
            Auditing = new AuditConfiguration(true, true, true, true, true, true)
        };

        return new SoftDeletionTestDbContext(options, configuration);
    }

    private async Task<(int AuthorId, int BlogId, int FirstPostId)> SeedGraphAsync()
    {
        var previousUserName = _currentUserName;
        _currentUserName = "seeder";

        await using var context = CreateContext();

        var firstTag = new SdTag { Name = "tag1" };
        var secondTag = new SdTag { Name = "tag2" };

        var blog = new SdBlog
        {
            Name = "blog",
            Address = new SdAddress { City = "Ankara" },
            Author = new SdAuthor { Name = "author" },
            Posts =
            [
                new SdPost { Title = "post1", Tags = [firstTag], Comments = [new SdComment { Text = "comment" }] },
                new SdPost { Title = "post2", Tags = [secondTag] },
            ],
            Notes = [new SdNote { Text = "note" }],
            Translations = [new SdBlogTranslation { Value = "translation" }],
            Attachments = [new SdAttachment { Name = "attachment" }],
            Links = [new SdBlogLink { Url = "link" }]
        };

        context.Blogs.Add(blog);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        _currentUserName = previousUserName;

        return (blog.AuthorId, blog.Id, blog.Posts[0].Id);
    }

    #endregion
}

#region Test model

public class SoftDeletionTestDbContext(DbContextOptions<SoftDeletionTestDbContext> options, IDataAccessConfiguration configuration) : MilvaDbContext(options, configuration)
{
    public DbSet<SdAuthor> Authors { get; set; }
    public DbSet<SdBlog> Blogs { get; set; }
    public DbSet<SdPost> Posts { get; set; }
    public DbSet<SdComment> Comments { get; set; }
    public DbSet<SdTag> Tags { get; set; }
    public DbSet<SdNote> Notes { get; set; }
    public DbSet<SdBlogTranslation> Translations { get; set; }
    public DbSet<SdAttachment> Attachments { get; set; }
    public DbSet<SdBlogLink> Links { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SdBlog>().OwnsOne(b => b.Address);
        modelBuilder.Entity<SdPost>().HasMany(p => p.Tags).WithMany(t => t.Posts);
        modelBuilder.Entity<SdNote>().HasOne(n => n.Blog).WithMany(b => b.Notes).HasForeignKey(n => n.BlogId).OnDelete(DeleteBehavior.ClientSetNull);
    }
}

public class SdAuthor : FullAuditableEntity<int>
{
    public string Name { get; set; }
    public List<SdBlog> Blogs { get; set; } = [];
}

public class SdBlog : FullAuditableEntity<int>
{
    public string Name { get; set; }
    public int AuthorId { get; set; }
    public SdAuthor Author { get; set; }
    public SdAddress Address { get; set; }
    public List<SdPost> Posts { get; set; } = [];
    public List<SdNote> Notes { get; set; } = [];
    public List<SdBlogTranslation> Translations { get; set; } = [];

    /// <summary> Optional (ClientSetNull) soft deletable children explicitly deleted with the blog. </summary>
    [CascadeOnDelete]
    public List<SdAttachment> Attachments { get; set; } = [];

    /// <summary> Optional (ClientSetNull) not soft deletable children explicitly deleted with the blog. </summary>
    [CascadeOnDelete]
    public List<SdBlogLink> Links { get; set; } = [];
}

public class SdAddress
{
    public string City { get; set; }
}

public class SdPost : FullAuditableEntity<int>
{
    public string Title { get; set; }
    public int BlogId { get; set; }
    public SdBlog Blog { get; set; }
    public List<SdTag> Tags { get; set; } = [];
    public List<SdComment> Comments { get; set; } = [];
}

public class SdComment : FullAuditableEntity<int>
{
    public string Text { get; set; }
    public int PostId { get; set; }
    public SdPost Post { get; set; }
}

public class SdTag : FullAuditableEntity<int>
{
    public string Name { get; set; }
    public List<SdPost> Posts { get; set; } = [];
}

public class SdNote : FullAuditableEntity<int>
{
    public string Text { get; set; }
    public int? BlogId { get; set; }
    public SdBlog Blog { get; set; }
}

/// <summary>
/// Not soft deletable required dependent.
/// </summary>
public class SdBlogTranslation
{
    public int Id { get; set; }
    public string Value { get; set; }
    public int BlogId { get; set; }
    public SdBlog Blog { get; set; }
}

public class SdAttachment : FullAuditableEntity<int>
{
    public string Name { get; set; }
    public int? BlogId { get; set; }
    public SdBlog Blog { get; set; }
}

public class SdBlogLink
{
    public int Id { get; set; }
    public string Url { get; set; }
    public int? BlogId { get; set; }
    public SdBlog Blog { get; set; }
}

public class SdBlogRepository(SoftDeletionTestDbContext dbContext) : BaseRepository<SdBlog, SoftDeletionTestDbContext>(dbContext);

#endregion
