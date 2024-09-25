using Npgsql;

namespace VegaExpress.Worker.Core.Utilities
{
    public class SQLExecutor: IDisposable
    {
        private readonly string _connectionString;
        private readonly NpgsqlConnection _connection;

        public SQLExecutor(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new NpgsqlConnection(_connectionString);
            _ = _connection.OpenAsync();
        }

        public async Task<int> ExecuteNonQuery(string sql, Dictionary<string, object> parameters)
        {                        
            await using var command = new NpgsqlCommand(sql, _connection);
            foreach (var (key, value) in parameters)
            {
                command.Parameters.AddWithValue(key, value);
            }
            return await command.ExecuteNonQueryAsync();
        }

        public async Task<T> ExecuteSingleReader<T>(string sql, Dictionary<string, object> parameters)
        {                        
            await using var command = new NpgsqlCommand(sql, _connection);
            foreach (var (key, value) in parameters)
            {
                command.Parameters.AddWithValue(key, value);
            }
            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return default(T)!; // No rows found
            }

            var result = MapRowToType<T>(reader);

            if (await reader.ReadAsync())
            {
                throw new InvalidOperationException("Expected a single row but found more.");
            }

            return result;
        }

        public async IAsyncEnumerable<Dictionary<string, object>> ExecuteReader(string sql, Dictionary<string, object> parameters = null!)
        {                        
            await using var command = new NpgsqlCommand(sql, _connection);
            if (parameters != null)
            {
                foreach (var (key, value) in parameters)
                {
                    command.Parameters.AddWithValue(key, value);
                }
            }
            await using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }
                yield return row;
            }
        }

        public async IAsyncEnumerable<T> ExecuteReader<T>(string sql, Dictionary<string, object> parameters = null!)
        {                        
            await using var command = new NpgsqlCommand(sql, _connection);
            if (parameters != null)
            {
                foreach (var (key, value) in parameters)
                {
                    command.Parameters.AddWithValue(key, value);
                }
            }
            await using var reader = await command.ExecuteReaderAsync();            
            while (await reader.ReadAsync())
            {
                var item = MapRowToType<T>(reader);
                yield return item;                
            }
        }        

        private static T MapRowToType<T>(NpgsqlDataReader reader)
        {
            var type = typeof(T);
            var properties = type.GetProperties();

            // Create a new instance of type T
            var target = Activator.CreateInstance(type);

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var propertyName = reader.GetName(i);
                var property = properties.FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

                if (property != null)
                {
                    var value = reader.GetValue(i);
                    if (value != DBNull.Value)
                    {
                        if (property.PropertyType.IsAssignableFrom(value.GetType()) ||
                            CanConvert(value, property.PropertyType))
                        {
                            property.SetValue(target, value);
                        }
                        else
                        {
                            Console.WriteLine($"Type mismatch for property '{propertyName}': expected {property.PropertyType}, got {value.GetType()}");
                        }
                    }
                }
            }

            return (T)target!;
        }

        private static bool CanConvert(object value, Type toType)
        {
            if (toType == typeof(string))
            {
                return true;
            }
            return Convert.ChangeType(value, toType, null) != null;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
