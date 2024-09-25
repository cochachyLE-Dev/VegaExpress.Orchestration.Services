using System.Reflection;

namespace VegaExpress.Worker.Core.Utilities
{
    public class SQLBuilder
    {
        private string _tableName;

        public SQLBuilder(string tableName)
        {
            _tableName = tableName;
        }

        public string BuildInsert(Dictionary<string, object> properties)
        {
            var columns = string.Join(",", properties.Keys);
            var parameters = string.Join(",", properties.Keys.Select(k => "@" + k));
            return $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters})";
        }

        public string BuildUpdate(Dictionary<string, object> properties, Dictionary<string, object> keys)
        {
            var updates = string.Join(",", properties.Where(p => !keys.ContainsKey(p.Key))
                                                     .Select(p => $"{p.Key} = @" + p.Key));
            var whereClause = string.Join(" AND ", keys.Select(k => $"{k.Key} = @" + k.Key));
            return $"UPDATE {_tableName} SET {updates} WHERE {whereClause}";
        }

        public string BuildDelete(Dictionary<string, object> keys)
        {
            var whereClause = string.Join(" AND ", keys.Select(k => $"{k.Key} = @" + k.Key));
            return $"DELETE FROM {_tableName} WHERE {whereClause}";
        }

        public string BuildSelectByKeys(Dictionary<string, object> properties, Dictionary<string, object> keys)
        {
            var columns = properties.Count > 0 ? string.Join(",", properties.Keys) : "*";
            var whereClause = string.Join(" AND ", keys.Select(k => $"{k.Key} = @" + k.Key));
            return $"SELECT {columns} FROM {_tableName} WHERE {whereClause}";
        }

        public string BuildSelectAll(Dictionary<string, object> properties = null!)
        {
            var columns = (properties == null || properties.Count > 0) ? string.Join(",", properties!.Keys) : "*";
            return $"SELECT {columns} FROM {_tableName}";
        }

        public string BuildSelectWithPagination(Dictionary<string, object> properties, int pageNumber, int pageSize)
        {
            var columns = properties.Count > 0 ? string.Join(",", properties.Keys) : "*";
            var offset = (pageNumber - 1) * pageSize;
            return $"SELECT {columns} FROM {_tableName} ORDER BY (SELECT NULL) LIMIT {pageSize} OFFSET {offset}";
        }
        public string BuildExistsByKeys(Dictionary<string, object> keys)
        {
            var whereClause = string.Join(" AND ", keys.Select(k => $"{k.Key} = @" + k.Key));
            return $"SELECT EXISTS(SELECT 1 FROM {_tableName} WHERE {whereClause})";
        }
        public string BuildContraintsPrimaryKey()
        {
            return $@"SELECT kcu.column_name
                    FROM information_schema.key_column_usage kcu
                        JOIN information_schema.table_constraints tc ON kcu.constraint_name = tc.constraint_name                        
                    WHERE tc.constraint_type = 'PRIMARY KEY'
                        AND tc.table_name = '{_tableName}';";
        }
        public Dictionary<string, object> GetParameters(Dictionary<string, object> properties, Dictionary<string, object> keys = null!)
        {
            var parameters = new Dictionary<string, object>();
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    parameters.Add("@" + prop.Key, prop.Value);
                }
            }
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    parameters.Add("@" + key.Key, key.Value);
                }
            }
            return parameters;
        }
    }
}
