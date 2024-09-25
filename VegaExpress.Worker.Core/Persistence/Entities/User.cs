namespace VegaExpress.Worker.Core.Persistence.Entities
{
    public class User
    {
        public string? UserUid { get; set; }
        public string? UserName { get; set; }
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set;}
    }
}