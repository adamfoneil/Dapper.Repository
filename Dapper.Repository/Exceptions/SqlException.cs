using System;
using System.Text.Json;

namespace Dapper.Repository.Exceptions
{
    public class SqlException : Exception
    {
        private const string modelKey = "model";

        public SqlException(string message, string sql, object model = null) : base(message)
        {
            Sql = sql;

            if (model != null)
            {
                Data.Add(modelKey, JsonSerializer.Serialize(model, options: new JsonSerializerOptions()
                {
                    WriteIndented = true
                }));
            }            
        }

        public string Sql { get; }

        public string ModelJson => Data.Contains(modelKey) ? Data[modelKey].ToString() : default;            
    }
}
