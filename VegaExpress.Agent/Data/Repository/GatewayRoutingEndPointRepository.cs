using System.Data.SQLite;
using VegaExpress.Agent.Data.Interfaces;
using VegaExpress.Agent.Data.Models;

namespace VegaExpress.Agent.Data.Repository
{
    internal class GatewayRoutingEndPointRepository : IGatewayRoutingEndPointRepository<GatewayRoutingEndPointModel>
    {
        private readonly SQLiteConnection _con = new SQLiteConnection(Constants.DbConstants.CONNECTION_STRING);

        public async Task<bool> CreateAsync(GatewayRoutingEndPointModel item)
        {
            try
            {
                await _con.OpenAsync();
                string query = "INSERT INTO gatewayRoutingEndPoints(iD,method,pattern,location,serviceUID)" +
                                " VALUES($iD,$method,$pattern,$location,$serviceUID);";
                var command = new SQLiteCommand(query, _con)
                {
                    Parameters =
                    {                        
                        new SQLiteParameter("iD", item.ID),
                        new SQLiteParameter("method", item.Method),
                        new SQLiteParameter("pattern", item.Pattern),
                        new SQLiteParameter("location", item.Location),
                        new SQLiteParameter("serviceUID", item.ServiceUID),
                    }
                };
                var result = await command.ExecuteNonQueryAsync();

                if (result != 0)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
            finally { _con.Close(); }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                await _con.OpenAsync();
                string query = $"DELETE FROM gatewayRoutingEndPoints WHERE id = {id}";
                var command = new SQLiteCommand(query, _con);
                var result = await command.ExecuteNonQueryAsync();
                if (result != 0)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
            finally { _con.Close(); }
        }

        public async Task<IEnumerable<GatewayRoutingEndPointModel>> GetAllAsync()
        {
            try
            {
                var services = new List<GatewayRoutingEndPointModel>();
                await _con.OpenAsync();
                string query = $"SELECT * FROM gatewayRoutingEndPoints";
                var command = new SQLiteCommand(query, _con);
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var service = new GatewayRoutingEndPointModel()
                    {
                        ServiceUID = reader.GetString(0),
                        ID = reader.GetInt32(1),
                        Method= reader.GetString(2),
                        Pattern= reader.GetString(3),
                        Location = reader.GetString(4)
                    };
                    services.Add(service);
                }
                return services;
            }
            catch
            {
                return new List<GatewayRoutingEndPointModel>();
            }
            finally { _con.Close(); }
        }

        public async Task<GatewayRoutingEndPointModel> GetByIdAsync(int id)
        {
            try
            {
                await _con.OpenAsync();
                string query = $"SELECT * FROM gatewayRoutingEndPoints WHERE id = {id}";
                var command = new SQLiteCommand(query, _con);
                var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var service = new GatewayRoutingEndPointModel()
                    {
                        ID = reader.GetInt32(1),
                        Method = reader.GetString(2),
                        Pattern = reader.GetString(3),
                        Location = reader.GetString(4),
                        ServiceUID = reader.GetString(5),
                    };
                    reader.Close();
                    return service;
                }
                else
                    return null!;
            }
            catch
            {

                return null!;
            }
            finally { _con.Close(); }
        }

        public async Task<bool> UpdateAsync(GatewayRoutingEndPointModel item)
        {
            try
            {
                await _con.OpenAsync();
                string query = $"UPDATE gatewayRoutingEndPoints SET method = $method, pattern = $pattern, location = $location, serviceUID = $serviceUID WHERE iD = '{item.ID}'";
                var command = new SQLiteCommand(query, _con)
                {
                    Parameters =
                    {                        
                        new SQLiteParameter("method", item.Method),
                        new SQLiteParameter("pattern", item.Pattern),
                        new SQLiteParameter("location", item.Location),
                        new SQLiteParameter("serviceUID", item.ServiceUID),
                    }
                };
                var result = await command.ExecuteNonQueryAsync();
                if (result != 0)
                    return true;
                else
                    return false;
            }
            catch
            {

                return false;
            }
            finally { _con.Close(); }
        }
    }
}
