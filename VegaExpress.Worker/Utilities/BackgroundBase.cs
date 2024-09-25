using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VegaExpress.Worker.Utilities
{
    public abstract class BackgroundBase : BackgroundService
    {
        protected virtual Task BeforeExecuteAsync() { return Task.CompletedTask; }
        protected virtual Task AfterExecuteAsync() {  return Task.CompletedTask; }
        protected abstract Task ExecuteWorkAsync(CancellationToken stoppingToken);
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await BeforeExecuteAsync();
            await ExecuteWorkAsync(stoppingToken);
            await AfterExecuteAsync();
        }
    }
}
