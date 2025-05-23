﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed", Justification = "<Pending>", Scope = "type", Target = "~T:Milvasoft.DataAccess.EfCore.Utils.IncludeLibrary.IIncludable`1")]
[assembly: SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed", Justification = "<Pending>", Scope = "type", Target = "~T:Milvasoft.DataAccess.EfCore.Utils.IncludeLibrary.IIncludable`2")]
[assembly: SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to \"static\" fields", Justification = "<Pending>", Scope = "member", Target = "~M:Milvasoft.DataAccess.EfCore.Configuration.DynamicFetchConfiguration.GetEntityAssembly~System.Reflection.Assembly")]
[assembly: SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "<Pending>", Scope = "member", Target = "~M:Milvasoft.DataAccess.EfCore.DbContextBase.ModelBuilderExtensions.UseUtcDateTime(Microsoft.EntityFrameworkCore.ModelBuilder)~Microsoft.EntityFrameworkCore.ModelBuilder")]
[assembly: SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "<Pending>", Scope = "member", Target = "~M:Milvasoft.DataAccess.EfCore.Utils.LookupModels.LookupRequestParameter.UpdateFilterByForTranslationPropertyNames(System.Collections.Generic.List{System.String})")]
[assembly: SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "<Pending>", Scope = "member", Target = "~M:Milvasoft.DataAccess.EfCore.RepositoryBase.Abstract.IBaseRepository`1.GetForDeleteAsync``1(System.Object,System.Func{Milvasoft.DataAccess.EfCore.Utils.IncludeLibrary.IIncludable{`0},Milvasoft.DataAccess.EfCore.Utils.IncludeLibrary.IIncludable},System.Linq.Expressions.Expression{System.Func{`0,System.Boolean}},System.Linq.Expressions.Expression{System.Func{`0,``0}},System.Linq.Expressions.Expression{System.Func{``0,System.Boolean}},System.Boolean,System.Boolean,System.Threading.CancellationToken)~System.Threading.Tasks.Task{``0}")]
[assembly: SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "<Pending>", Scope = "member", Target = "~M:Milvasoft.DataAccess.EfCore.DbContextBase.MilvaDbContext.SoftDelete(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry)")]
