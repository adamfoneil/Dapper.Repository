using System;
using System.Text.Json;

namespace Dapper.Repository.Exceptions
{
    public class RepositoryException : Exception
    {
        private const string modelKey = "model";
        private const string sqlKey = "sql";

        public RepositoryException(string message, string sql, object model = null) : base(message)
        {
            Data.Add(sqlKey, sql);

            if (model != null)
            {
                Data.Add(modelKey, JsonSerializer.Serialize(model, options: new JsonSerializerOptions()
                {
                    WriteIndented = true
                }));
            }            
        }

        public string Sql => Data[sqlKey].ToString();

        public string ModelJson => Data.Contains(modelKey) ? Data[modelKey].ToString() : default;            
    }
}
