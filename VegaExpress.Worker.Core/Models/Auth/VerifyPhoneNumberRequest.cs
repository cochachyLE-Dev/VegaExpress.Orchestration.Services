using System.ComponentModel.DataAnnotations;

namespace VegaExpress.Worker.Core.Models.Auth
{
    public class VerifyPhoneNumberRequest
    {
        [Required]
        public string? PhoneNumber { get; set; }
        [Required]
        public string? Code { get; set; }
    }
}