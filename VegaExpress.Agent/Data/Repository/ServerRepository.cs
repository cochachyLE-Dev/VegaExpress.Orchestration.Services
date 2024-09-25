using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VegaExpress.Agent.Data.Interfaces;
using VegaExpress.Agent.Data.Models;

namespace VegaExpress.Agent.Data.Repository
{
    internal class ServerRepository : IServerRepository
    {
        public Task<bool> CreateAsync(ServerModel entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ServerModel>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ServerModel> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(ServerModel entity)
        {
            throw new NotImplementedException();
        }
    }
}
