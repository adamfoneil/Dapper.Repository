using AO.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Repository
{
    public class BusinessLogic<TModel>
    {
        public virtual async Task<bool> AllowSaveAsync() => await Task.FromResult(true);
     
    }
}
