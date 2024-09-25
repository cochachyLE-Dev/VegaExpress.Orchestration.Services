using System.Data.SQLite;
using VegaExpress.Agent.Data.Interfaces;
using VegaExpress.Agent.Data.Models;
using VegaExpress.Agent.Shared;

namespace VegaExpress.Agent.Data.Repository
{
    internal class WorkerServiceRepository : IWorkerServiceRepository
    {
        private readonly SQLiteConnection _con = new SQLiteConnection(Constants.DbConstants.CONNECTION_STRING);

        public async Task<bool> CreateAsync(WorkerServiceModel item)
        {
            try
            {
                await _con.OpenAsync();
                string query = "INSERT INTO Services(serviceUID,serviceName,serviceVersion,serviceAddress,servicePortRange,gatewayServiceAddress,serviceTrafficLimit,serviceErrorRateLimit,serviceInstanceLimit,serviceLocation,serviceState,color)" +
                                " VALUES($serviceUID,$serviceName,$serviceVersion,$serviceAddress,$servicePortRange,$gatewayServiceAddress,$serviceTrafficLimit,$serviceErrorRateLimit,$serviceInstanceLimit,$serviceLocation,$serviceState,$color);";
                var command = new SQLiteCommand(query, _con)
                {
                    Parameters =
                    {
                        new SQLiteParameter("serviceUID", item.ServiceUID),
                        new SQLiteParameter("serviceName", item.ServiceName),
                        new SQLiteParameter("serviceVersion", item.ServiceVersion),
                        new SQLiteParameter("serviceAddress", item.ServiceAddress),
                        new SQLiteParameter("servicePortRange", item.ServicePortRange),
                        new SQLiteParameter("gatewayServiceAddress", item.GatewayServiceAddress),
                        new SQLiteParameter("serviceTrafficLimit", item.ServiceTrafficLimit),
                        new SQLiteParameter("serviceErrorRateLimit", item.ServiceErrorRateLimit),
                        new SQLiteParameter("serviceInstanceLimit", item.ServiceInstanceLimit),
                        new SQLiteParameter("serviceLocation", item.ServiceLocation),
                        new SQLiteParameter("serviceState", item.ServiceStartType),
                        new SQLiteParameter("color", item.Color),
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

        public async Task<bool> DeleteAsync(string serviceUid)
        {
            try
            {
                await _con.OpenAsync();
                string query = $"DELETE FROM services WHERE serviceUid = {serviceUid}";
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

        public async Task<IEnumerable<WorkerServiceModel>> GetAllAsync()
        {
            try
            {
                var services = new List<WorkerServiceModel>();
                await _con.OpenAsync();
                string query = $"SELECT * FROM services";
                var command = new SQLiteCommand(query, _con);
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var service = new WorkerServiceModel()
                    {
                        ServiceUID = reader.GetString(0),
                        ServiceName = reader.GetString(1),
                        ServiceVersion = reader.GetString(2),
                        ServiceAddress = reader.GetString(3),
                        ServicePortRange = reader.GetString(4),
                        GatewayServiceAddress = reader.GetString(5),
                        ServiceTrafficLimit = Convert.ToInt32(reader.GetString(6)),
                        ServiceErrorRateLimit = Convert.ToInt32(reader.GetString(7)),
                        ServiceInstanceLimit = Convert.ToInt32(reader.GetString(8)),
                        ServiceLocation = reader.GetString(9),
                        ServiceStartType = reader.GetString(10),
                        Color = reader.GetString(11)
                    };
                    services.Add(service);
                }
                return services;
            }
            catch
            {
                return new List<WorkerServiceModel>();
            }
            finally { _con.Close(); }
        }

        public async Task<WorkerServiceModel> GetByIdAsync(string serviceUid)
        {
            try
            {
                await _con.OpenAsync();
                string query = $"SELECT * FROM services WHERE serviceUid = '{serviceUid}'";
                var command = new SQLiteCommand(query, _con);
                var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var service = new WorkerServiceModel()
                    {
                        ServiceUID = reader.GetString(0),
                        ServiceName = reader.GetString(1),
                        ServiceVersion = reader.GetString(2),
                        ServiceAddress = reader.GetString(3),
                        ServicePortRange = reader.GetString(4),
                        GatewayServiceAddress = reader.GetString(5),
                        ServiceTrafficLimit = Convert.ToInt32(reader.GetString(6)),
                        ServiceErrorRateLimit = Convert.ToInt32(reader.GetString(7)),
                        ServiceInstanceLimit = Convert.ToInt32(reader.GetString(8)),
                        ServiceLocation = reader.GetString(9),
                        ServiceStartType = reader.GetString(10),
                        Color = reader.GetString(11)
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

        public async Task<bool> UpdateAsync(WorkerServiceModel item)
        {
            try
            {
                await _con.OpenAsync();
                string query = $"UPDATE services SET serviceName = $serviceName,serviceVersion = $serviceVersion,serviceAddress = $serviceAddress,servicePortRange = $servicePortRange, gatewayServiceAddress = $gatewayServiceAddress, serviceTrafficLimit = $serviceTrafficLimit,serviceErrorRateLimit = $serviceErrorRateLimit,serviceInstanceLimit = $serviceInstanceLimit,serviceLocation = $serviceLocation,serviceState = $serviceState,color = $color WHERE serviceUID = '{item.ServiceUID}'";
                var command = new SQLiteCommand(query, _con)
                {
                    Parameters =
                    {
                        new SQLiteParameter("serviceName", item.ServiceName),
                        new SQLiteParameter("serviceVersion", item.ServiceVersion),
                        new SQLiteParameter("serviceAddress", item.ServiceAddress),
                        new SQLiteParameter("servicePortRange", item.ServicePortRange),
                        new SQLiteParameter("gatewayServiceAddress", item.GatewayServiceAddress),
                        new SQLiteParameter("serviceTrafficLimit", item.ServiceTrafficLimit),
                        new SQLiteParameter("serviceErrorRateLimit", item.ServiceErrorRateLimit),
                        new SQLiteParameter("serviceInstanceLimit", item.ServiceInstanceLimit),
                        new SQLiteParameter("serviceLocation", item.ServiceLocation),
                        new SQLiteParameter("serviceState", item.ServiceStartType),
                        new SQLiteParameter("color", item.Color),
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