I've been critical of the repository pattern in the past because I've seen it lead to verbosity and repeated code. But there's no inherent reason the repository pattern should fall prey to this. I'm revisiting this now because my [Dapper.CX](https://github.com/adamfoneil/Dapper.CX) project has been getting complicated. Once again I'm feeling the need to get back to basics, rethink the dependency footprint and my approach to business logic.

The idea now will be to write data access code like this:

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
## Architecture
- [DbContext](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/DbContext.cs) provides the low-level connection infrastracture and some SQL dialect options. This will provide the basis for database-specific implementations, namely [SqlServerContext](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository.SqlServer/SqlServerContext.cs). This is what you'd inherit from in your project. Note, this is not to be confused with Entity Framework's DbContext object.
- Your model classes must implement [IModel&lt;TKey&gt;](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IModel.cs), from [AO.Models](https://www.nuget.org/packages/AO.Models/1.1.39). This is different from my CX project which didn't have a dependency like this. I figure this is a pretty minimal requirement, and a straightforward way to ensure that all models have an `Id` property with a consistent type.
- [Repository](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/Repository.cs) provides CRUD methods like Get, Save, Delete, Merge, and so on. You'd inherit from this in your project, typically creating a base repository that implements your conventions (say, around auditing, timestamps, and such). For tables that need unique business logic, you'd create a repository just for them with specific [overrides](https://github.com/adamfoneil/Dapper.Repository/blob/master/Dapper.Repository/Repository_virtuals.cs) of default behavior. For tables with no special requirements, you can use a more standard or generic repository.
- Your `DbContext` would have repositories added that represent access to each table in your database.

```csharp
public class BaseRepository<TModel, TKey> : Repository<TModel, TKey>
{
    // implement all my model-wide conventions
}

// create a dedicated repository class for each table, adding whatever is special about it by overriding crud methods as necessary.
// If a table has nothing unique about it (no special behavior or rules), then it may use BaseRepository directly
public class EmployeeRepository : BaseRepository<Employee, int>
{
}

// lastly, create a context object that provides access to all your repositories.
// this is what you'd inject in your IoC container and access throughout your application
public MyContext : SqlServerContext
{
    public MyContext(string connectionString, IUserBase user) : base(connectionstring, user)
    {
    }
    
    // assumes that the Appointment table has nothing special about it
    public BaseRepository<Appointment, int> Appointments => new BaseRepository<Appointment, int>(this); 
    
    // special repository because for some reason it has special validation or other behavior
    public EmployeeRepository Employees => new EmployeeRepository(this); 
}
```

## Issues with Dapper.CX
The main issue I'm having with Dapper.CX is that there's not a good place for business logic such as validation, tenant isolation, permission checks, navigation properties, and trigger-like behavior. My solution in the past has been to offer a bunch of [interfaces](https://github.com/adamfoneil/Models/tree/master/Models/Interfaces) from AO.Models that you would implement directly on your model classes. I like this opt-in approach in theory, but there are two problems.

- The logic for supporting all this custom behavior is embedded in the low-level [CRUD provider](https://github.com/adamfoneil/Dapper.CX/blob/master/Dapper.CX.Base/Abstract/SqlCrudProvider.cs) itself. If you read through this class, you'll see a lot of business-logic-like things in many places for validation, trigger execution, auditing, change tracking, and so on. This is a lot of complexity and coupling where I don't think it belongs.
- Some of these interfaces like [IGetRelated](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IGetRelated.cs), [ITrigger](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/ITrigger.cs), [IValidate](https://github.com/adamfoneil/Models/blob/master/Models/Interfaces/IValidate.cs) have `IDbConnection` arguments. This forces a database dependency in your model layer. You can avoid this by playing an elaborate game with partial classes and linked source, but this is hard to manage.
