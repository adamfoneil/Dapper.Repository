[![Build Status](https://ci.appveyor.com/api/projects/status/3opr5fvqudspcioh?svg=true)](https://ci.appveyor.com/project/adamosoftware/dapper-repository)
[![Nuget](https://img.shields.io/nuget/v/AO.Dapper.Repository.SqlServer)](https://www.nuget.org/packages/AO.Dapper.Repository.SqlServer/)

This library lets you write data access code like this:

```csharp
public class SomeController : Controller
{
    private readonly SqlServerContext _context;
    
    public SomeController(SqlServerContext context)
    {
        _context = context;
    }
    
    public async Task Edit(int id)
    {
        var row = await _context.Employees.GetAsync(id);
        return View(row);
    }
    
    public async Task Save(Employee employee)
    {
        await _context.Employees.SaveAsync(employee);
        return Redirect("Edit", new { id = employee.Id });
    }
}
```
The [integration tests](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/SqlServerIntegration.cs) provide examples that give more context:

0. Install NuGet package [AO.Dapper.Repository.SqlServer](https://www.nuget.org/packages/AO.Dapper.Repository.SqlServer/)
1. Before you start, you should already have a number of model classes. My tests work with these [examples](https://github.com/adamfoneil/Dapper.Repository/tree/master/Dapper.Repository.Test.Models).
2. Create a class based on `SqlServerContext<TUser>` that will provide the access point to all your repositories. Example: [MyContext](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/MyContext.cs). You pass your database connection string, current user name, and an `ILogger`. My example uses a localdb connection string for test purposes. In a real application, it would typically come from your configuration in some way. Optionally, but most often, you'll need to override [QueryUserInfo](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/MyContext.cs#L23) so that you can access properties of the current user in your crud operations. More on this below.
3. Create a `Repository` class that handles your common data access scenario. Example: [BaseRepository](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Repositories/BaseRepository.cs). My example assumes an `int` key type, and overrides the [BeforeSaveAsync](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/Repository_virtuals.cs#L41) method to capture user and timestamp info during inserts and updates.
4. For models that require unique behavior, validation, or trigger-like behavior, create repository classes specifically for them. You would typically inherit from your own `BaseRepository` so as to preserve any existing conventional behavior. Example: [WorkHoursRepository](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Repositories/WorkHoursRepository.cs). Note, there are many overrides you can implement for various crud events, found [here](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/Repository_virtuals.cs).
5. Add your repository classes as read-only properties of your `DbContext`, for example [here](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/MyContext.cs#L28-L37). Note, I have more model classes than repositories because I'm lazy, and don't need them for a working demo.

## Architecture
- [DbContext](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/DbContext.cs) provides the low-level connection infrastracture and some SQL dialect options. This will provide the basis for database-specific implementations, namely [SqlServerContext](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.SqlServer/SqlServerContext.cs). This is what you'd inherit from in your project and inject in your components and controllers. This is not to be confused with Entity Framework's DbContext object. I risked likely confusion here because I think "DbContext" is a good name for what this does.
- Your model classes must implement [IModel&lt;TKey&gt;](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IModel.cs), from [AO.Models](https://www.nuget.org/packages/AO.Models/1.1.39). This is different from my CX project which didn't have a dependency like this. I figure this is a pretty minimal requirement, and a straightforward way to ensure that all models have an `Id` property with a consistent type.
- [Repository](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/Repository.cs) provides CRUD methods like Get, Save, Delete, Merge, and so on. You'd inherit from this in your project, typically creating a base repository that implements your conventions (say, around auditing, timestamps, and such). For tables that need unique business logic, you'd create a repository just for them with specific [overrides](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/Repository_virtuals.cs) of default behavior. For tables with no special requirements, you can use a more standard or generic repository.
- The low-level SQL generation happens in AO.Models [SqlBuilder](https://github.com/adamfoneil/Models/blob/master/Models/Static/SqlBuilder.cs), which uses reflection to analyze C# objects and properties to derive SQL.
- Your `DbContext` would have repositories added that represent access to each table in your database, as in this [example](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/MyContext.cs#L17-L26).

## Working With TUser
Most applications will have authentication and need to track database operations by user in some way. When you create your `DbContext` object, you must provide a `TUser` that represents the current user. In the tests, I have a very simple [User](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/User.cs) type.

In your application, you should override [QueryUserAsync](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/DbContext.cs#L39) so that all your user properties are available to crud operations. This is usually necessary for determining permissions or verifying tenant isolation, for example.

A straightforward override of `QueryUserAsync` could be a database query like any other. Bear in mind, however, that this may be called very often in your application, which is not very scalable. I'm working on some examples of this that will use [IDistributedCache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-5.0) in some way, so stay tuned.

## Background
I've been critical of the repository pattern in the past because I've seen it lead to verbosity and repeated code. But there's no inherent reason the repository pattern should fall prey to this. I'm revisiting this now because my [Dapper.CX](https://github.com/adamfoneil/Dapper.CX) project has been getting complicated. Once again I'm feeling the need to get back to basics, rethink the dependency footprint and my approach to business logic.

The main issue I'm having with Dapper.CX is that there's not a good place for business logic such as validation, tenant isolation, permission checks, navigation properties, and trigger-like behavior. My solution in the past has been to offer a bunch of [interfaces](https://github.com/adamfoneil/Models/tree/master/Models/Interfaces) from AO.Models that you would implement directly on your model classes. I like this opt-in approach in theory, but there are two problems.

- The logic for supporting all this custom behavior is embedded in the low-level [CRUD provider](https://github.com/adamfoneil/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs) itself. If you read through this class, you'll see a lot of business-logic-like things in many places for validation, trigger execution, auditing, change tracking, and so on. This is a lot of complexity and coupling where I don't think it belongs.
- Some of these interfaces like [IGetRelated](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IGetRelated.cs), [ITrigger](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/ITrigger.cs), [IValidate](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IValidate.cs) have `IDbConnection` arguments. This forces a database dependency in your model layer. You can avoid this by playing an elaborate game with partial classes and linked source, but this is hard to manage.
