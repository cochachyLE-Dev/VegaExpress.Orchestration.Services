using System.ComponentModel.DataAnnotations;

namespace VegaExpress.Worker.Core.Models.Auth
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
