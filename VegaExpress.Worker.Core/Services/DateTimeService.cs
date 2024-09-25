using VegaExpress.Worker.Core.Services.Contracts;

namespace VegaExpress.Worker.Core.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime Now => DateTime.Now;
    }
}
