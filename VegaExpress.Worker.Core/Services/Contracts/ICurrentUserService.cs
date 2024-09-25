namespace VegaExpress.Worker.Core.Services.Contracts
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        string UserName { get; }
        string Email { get; }
    }
}
