using AO.Models;

namespace BlazorAO.Models.Conventions
{
    [Schema("app")]
    [Identity(nameof(Id))]
    public abstract class AppTable
    {
        public int Id { get; set; }
    }
}
