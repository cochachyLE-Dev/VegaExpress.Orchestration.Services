﻿using System.Data.SQLite;

using VegaExpress.Agent.Data.Enums;
using VegaExpress.Agent.Data.Interfaces;
using VegaExpress.Agent.Data.Models;
using VegaExpress.Agent.Generated;


namespace VegaExpress.Agent.Data.Repository
{
    internal class GatewayRoutingServiceRepository : IGatewayRoutingServiceRepository
    {
        private readonly SQLiteConnection _con = new SQLiteConnection(Constants.DbConstants.CONNECTION_STRING);

        public async Task<bool> CreateAsync(GatewayRoutingServiceModel item)
        {
            try
            {
                await _con.OpenAsync();
                string query = "INSERT INTO gatewayRoutingServiceInstances(serviceUID,serviceName,serviceLocation,serviceVersion,serviceAddress,processName,pID,lastSession,serviceStatus)" +
                                " VALUES($serviceUID,$serviceName,$serviceLocation,$serviceVersion,$serviceAddress,$processName,$pID,$lastSession,$serviceStatus);";
                var command = new SQLiteCommand(query, _con)
                {
                    Parameters =
                    {
                        new SQLiteParameter("serviceUID", item.ServiceUID),
                        new SQLiteParameter("serviceName", item.ServiceName),
                        new SQLiteParameter("serviceLocation", item.ServiceLocation ?? "/"),
                        new SQLiteParameter("serviceVersion", item.ServiceVersion),
                        new SQLiteParameter("serviceAddress", item.ServiceAddress),
                        new SQLiteParameter("processName", item.ProcessName ?? ""),
                        new SQLiteParameter("pID", item.ProcessID),
                        new SQLiteParameter("lastSession", item.LatestSession),
                        new SQLiteParameter("serviceStatus", (int)item.ServiceState)
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
                string query = $"DELETE FROM gatewayRoutingServiceInstances WHERE serviceUid = {serviceUid}";
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

        public async Task<IEnumerable<GatewayRoutingServiceModel>> GetAllAsync()
        {
            try
            {
                var services = new List<GatewayRoutingServiceModel>();
                await _con.OpenAsync();
                string query = $"SELECT * FROM gatewayRoutingServiceInstances";
                var command = new SQLiteCommand(query, _con);
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    var service = new GatewayRoutingServiceModel()
                    {
                        ServiceUID = reader.GetString(0),
                        ServiceName = reader.GetString(1),
                        ServiceLocation = reader.GetString(2),
                        ServiceVersion = reader.GetString(3),
                        ServiceAddress = reader.GetString(4),
                        ProcessName = reader.GetString(5),
                        ProcessID = reader.GetInt32(6),
                        ServiceState = (ServiceState)reader.GetInt32(7),
                        ServiceAgentUID = reader.GetString(8),
                        LatestSession = reader.GetDateTime(9),
                    };
                    services.Add(service);
                }
                return services;
            }
            catch
            {
                return new List<GatewayRoutingServiceModel>();
            }
            finally { _con.Close(); }
        }

        public async Task<GatewayRoutingServiceModel> GetByIdAsync(string serviceUid)
        {
            try
            {
                await _con.OpenAsync();
                string query = $"SELECT * FROM gatewayRoutingServiceInstances WHERE serviceUid = {serviceUid}";
                var command = new SQLiteCommand(query, _con);
                var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var service = new GatewayRoutingServiceModel()
                    {
                        ServiceUID = reader.GetString(0),
                        ServiceName = reader.GetString(1),
                        ServiceLocation = reader.GetString(2),
                        ServiceVersion = reader.GetString(3),
                        ServiceAddress = reader.GetString(4),
                        ProcessName = reader.GetString(5),
                        ProcessID = reader.GetInt32(6),
                        LatestSession = reader.GetDateTime(7),
                        ServiceState = (ServiceState)reader.GetInt32(8)
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

        public async Task<bool> UpdateAsync(GatewayRoutingServiceModel item)
        {
            try
            {
                await _con.OpenAsync();
                string query = $"UPDATE gatewayRoutingServiceInstances SET serviceName = $serviceName,serviceLocation = $serviceLocation,serviceVersion = $serviceVersion,serviceAddress = $serviceAddress,processName = $processName,pID = $pID,lastSession = $lastSession,serviceStatus = $serviceStatus WHERE serviceUID = '{item.ServiceUID}'";
                var command = new SQLiteCommand(query, _con)
                {
                    Parameters =
                    {
                        new SQLiteParameter("serviceName", item.ServiceName),
                        new SQLiteParameter("serviceLocation", item.ServiceLocation),
                        new SQLiteParameter("serviceVersion", item.ServiceVersion),
                        new SQLiteParameter("serviceAddress", item.ServiceAddress),
                        new SQLiteParameter("processName", item.ProcessName),
                        new SQLiteParameter("pID", item.ProcessID),
                        new SQLiteParameter("lastSession", item.LatestSession),
                        new SQLiteParameter("serviceStatus", (int)item.ServiceState)
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
