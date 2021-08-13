I've been critical of the repository pattern in the past because I've seen it lead to verbosity and repeated code. But there's no inherent reason the repository pattern should fall prey to this. I'm revisiting this now because my [Dapper.CX](https://github.com/adamfoneil/Dapper.CX) project has been getting complicated. Once again I'm feeling the need to get back to basics, rethink the dependency footprint and my approach to extensibility.

The main issue I'm having with Dapper.CX is that there's not a good place for business logic.

The idea now will be to write data access code like this:

```csharp
public class SomeController : Controller
{
    private readonly Context _context;
    
    public SomeController(Context context)
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

