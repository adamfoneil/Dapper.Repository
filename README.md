[![Build Status](https://ci.appveyor.com/api/projects/status/3opr5fvqudspcioh?svg=true)](https://ci.appveyor.com/project/adamosoftware/dapper-repository)
[![Nuget](https://img.shields.io/nuget/v/AO.Dapper.Repository.SqlServer)](https://www.nuget.org/packages/AO.Dapper.Repository.SqlServer/)

This library lets you write data access code that offers:
- an IoC-friendly single point of access to all your repository classes, keeping your constructors simple throughout your application
- a way to implement model-wide conventions along with table-specific business logic where needed
- connection [extension methods](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.SqlServer/Extensions/SqlServerExtensions.cs) for simple entity access
- efficient, typed user profile access

To implement, bear in mind:
- Your model classes must implement [IModel](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IModel.cs) from package [AO.Models](https://www.nuget.org/packages/AO.Models), installed automatically as a dependency.
- As a Dapper-based library, this uses direct database connections. As such, this works only on the backend -- such as in a Blazor Server app, API backend, or MVC/Razor Pages app.

Example, using a fictional `MyContext` object:

```csharp
public class SomeController : Controller
{
    private readonly MyContext _context;
    
    public SomeController(MyContext context)
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
The [integration tests](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Tests/SqlServerIntegration.cs) provide examples that give more context, but here's how to get started:

0. Install NuGet package [AO.Dapper.Repository.SqlServer](https://www.nuget.org/packages/AO.Dapper.Repository.SqlServer/)
1. Implement your model classes. My tests work with these [examples](https://github.com/adamfoneil/Dapper.Repository/tree/master/Dapper.Repository.Test.Models). Your model classes must implement [IModel](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IModel.cs) from package [AO.Models](https://www.nuget.org/packages/AO.Models).
2. Create a class based on `SqlServerContext<TUser>` that will provide the access point to all your repositories. Example: [DataContext](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Contexts/DataContext.cs). You pass your database connection string, current user name, and an `ILogger`. My example uses a localdb connection string for test purposes. In a real application, it would typically come from your configuration in some way. Optionally, but most often, you'll need to override [QueryUserInfo](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Contexts/DataContext.cs#L39) so that you can access properties of the current user in your crud operations. More on this below.
3. Create a `Repository` class that handles your common data access scenario. Example: [BaseRepository](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Repositories/BaseRepository.cs). My example assumes an `int` key type, and overrides the [BeforeSaveAsync](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/Repository_virtuals.cs#L41) method to capture user and timestamp info during inserts and updates.
4. For models that require unique behavior, validation, or trigger-like behavior, create repository classes specifically for them. You would typically inherit from your own `BaseRepository` so as to preserve any existing conventional behavior. Example: [WorkHoursRepository](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Repositories/WorkHoursRepository.cs). Note, there are many overrides you can implement for various crud events, found [here](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/Repository_virtuals.cs).
5. Add your repository classes as read-only properties of your `DataContext`, for example [here](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Contexts/DataContext.cs#L71-L80). Note, I have more model classes than repositories because I'm lazy, and don't need them for a working demo.
6. Add your `DataContext` object to your `services` collection in startup. A Blazor Server approach might look like this:
```csharp
services.AddScoped((sp) =>
{
    var authState = sp.GetRequiredService<AuthenticationStateProvider>();                
    var logger = sp.GetRequiredService<ILogger<DataContext>>();
    var cache = sp.GetRequiredService<IDistributedCache>();
    return new DataContext(connectionString, cache, authState, logger);
});
```
Since you create your own `DataContext` object, you can decide what dependencies are useful to pass to it. The `AuthenticationStateProvider` is used to get the current user name during `QueryUserInfo`. The `IDistributedCache` is used to avoid a database roundtrip to get user details, used [here](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Contexts/DataContext.cs#L44). The `ILogger` is required by the low-level [DbContext](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/DbContext.cs#L9) object. This is so SQL errors have a place to be [logged consistently](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/Repository.cs#L125).

## Working With TUser
Most applications will have authentication and need to track database operations by user in some way. When you create your `DbContext` object, you must provide a `TUser` that represents the current user. You also override the [QueryUserAsync](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/DbContext.cs#L48) method, implementing any database query and/or cache access that makes sense in your application.

The test project uses [DataContext](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Contexts/DataContext.cs) where `TUser` is [UserInfoResult](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Queries/UserInfo.cs#L10). Notice how the `QueryUserAsync` [override](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Contexts/DataContext.cs#L39) checks in a cache for the user, then queries the database if it's not found. This is how you achieve efficient, typed user profile access in your applications.

## Blazor Server Example
I don't have an open source Blazor Server app example now, but here's a code sample to show how to implement in `Startup`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    var connectionString = Configuration.GetConnectionString("DefaultConnection");

    // typical Identity stuff
    services
        .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
    services
        .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>();                

    // Blazor boilerplate
    services.AddRazorPages();
    services.AddServerSideBlazor();
    services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
    services.AddDatabaseDeveloperPageExceptionFilter();
    
    // add a cache appropriate for dev/test scenarios. In production, consider something truly distributed
    services.AddDistributedMemoryCache();

    // Dapper.Repository specific
    services.AddScoped((sp) =>
    {
        var authState = sp.GetRequiredService<AuthenticationStateProvider>();                
        var logger = sp.GetRequiredService<ILogger<DataContext>>();
        var cache = sp.GetRequiredService<IDistributedCache>();
        return new DataContext(connectionString, cache, authState, logger);
    });
}
```
The `DataContext` class referenced above:

```csharp
public class DataContext : SqlServerContext<UserInfoResult>
{
    private readonly AuthenticationStateProvider _authState;
    private readonly IDistributedCache _cache;

    public DataContext(string connectionString, IDistributedCache cache, AuthenticationStateProvider authState, ILogger logger)  : base(connectionString, logger)
    {
        _authState = authState;
        _cache = cache;
    }

    protected override async Task<UserInfoResult> QueryUserAsync(IDbConnection connection)
    {
        var authState = await _authState.GetAuthenticationStateAsync();
        var userName = authState.User.Identity.Name;
        if (string.IsNullOrEmpty(userName)) return null;

        var key = $"userInfo.{userName}";
        var result = await _cache.GetItemAsync<UserInfoResult>(key);

        if (result == default(UserInfoResult))
        {
            result = await new UserInfo() { UserName = userName }.ExecuteSingleOrDefaultAsync(connection);
            await _cache.SetItemAsync(key, result);
        }

        return result;
    }

    // repository classes follow...
}
```
A few points to note about the code above:
- The cache access methods you see `GetItemAsync` and `SetItemAsync` are extensions you can find [here](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Extensions/DistributedCacheExtensions.cs) that aren't part of the Dapper.Repository package proper.
- The line `await new UserInfo() { UserName = userName }.ExecuteSingleOrDefaultAsync(connection)` executes a SQL query via a wrapper class `UserInfo`. This functionality comes from my [Dapper.QX](https://github.com/adamfoneil/Dapper.QX) library. The integration tests [here](https://github.com/adamfoneil/Dapper.Repository/tree/master/Dapper.Repository.Test/Queries) use this also.

## Classic Extension Methods
If you need an easy way to perform CRUD operations on model types without any intermediary business logic, there are some "classic" [extension methods](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.SqlServer/Extensions/SqlServerExtensions.cs) for this. Most of these do not require `IModel` except for `SaveAsync`:

- Task\<TModel\> [GetAsync](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.SqlServer/Extensions/SqlServerExtensions.cs#L16)<TKey>
 (this IDbConnection connection, TKey id, [ string identityColumn ], [ IDbTransaction txn ])
- Task\<TModel\> [GetWhereAsync](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.SqlServer/Extensions/SqlServerExtensions.cs#L19)
 (this IDbConnection connection, object criteria, [ IDbTransaction txn ])
- Task\<TModel\> [InsertAsync](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.SqlServer/Extensions/SqlServerExtensions.cs#L22)<TModel>
 (this IDbConnection connection, TModel model, [ IEnumerable<string> columnNames ], [ string identityColumn ], [ Action<TModel, TKey> afterInsert ], [ IDbTransaction txn ])
- Task [UpdateAsync](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.SqlServer/Extensions/SqlServerExtensions.cs#L25)<TModel>
 (this IDbConnection connection, TModel model, [ IEnumerable<string> columnNames ], [ string identityColumn ], [ IDbTransaction txn ])
- Task [DeleteAsync](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.SqlServer/Extensions/SqlServerExtensions.cs#L28)<TKey>
 (this IDbConnection connection, TKey id, [ string identityColumn ], [ string tableName ], [ IDbTransaction txn ])
- Task\<TModel\> [SaveAsync](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.SqlServer/Extensions/SqlServerExtensions.cs#L31)<TModel>
 (this IDbConnection connection, TModel model, [ IEnumerable<string> columnNames ], [ string identityColumn ], [ IDbTransaction txn ])
- Task\<TModel\> [MergeAsync](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.SqlServer/Extensions/SqlServerExtensions.cs#L42)<TModel>
 (this IDbConnection connection, TModel model, [ Action<TModel, TModel> onExisting ], [ IDbTransaction txn ])

## Background
I've been critical of the repository pattern in the past because I've seen it lead to verbosity and repeated code. But there's no inherent reason it has to be this way. I'm revisiting this now because my [Dapper.CX](https://github.com/adamfoneil/Dapper.CX) project has been getting complicated. Once again I'm feeling the need to get back to basics, rethink the dependency footprint and my approach to business logic.

The main issue I'm having with Dapper.CX is that there's not a good place for business logic such as validation, tenant isolation, permission checks, navigation properties, and trigger-like behavior. My solution in the past has been to offer a bunch of [interfaces](https://github.com/adamfoneil/Models/tree/master/Models/Interfaces) from AO.Models that you would implement directly on your model classes. I like this opt-in approach in theory, but there are two problems.

- The logic for supporting all this custom behavior is embedded in the low-level [CRUD provider](https://github.com/adamfoneil/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs) itself. If you read through this class, you'll see a lot of business-logic-like things in many places for validation, trigger execution, auditing, change tracking, and so on. This is a lot of complexity and coupling where I don't think it belongs.
- Some of these interfaces like [IGetRelated](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IGetRelated.cs), [ITrigger](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/ITrigger.cs), [IValidate](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IValidate.cs) have `IDbConnection` arguments. This forces a database dependency in your model layer. You can avoid this by playing an elaborate game with partial classes and linked source, but this is hard to manage.

Another issue worth mentioning is that the .NET Core integration approach I used in Dapper.CX was a little clumsy the way it queried the current user as [part of the DI setup](https://github.com/adamfoneil/Dapper.CX/wiki/Using-Dapper.CX-with-Dependency-Injection#setting-the-user-property). I've moved that query to the async `QueryUserAsync` virtual method. I've also removed the `IUserBase` requirement from `TUser`. `IUserBase` was meant to ensure that you have access to the user's current local time. That's fine if you want that (and I [use](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.Test/Queries/UserInfo.cs#L18) that in my test app), but there's no requirement for it going forward.
