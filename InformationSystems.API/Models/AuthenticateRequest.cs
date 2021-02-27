using System.ComponentModel.DataAnnotations;

namespace InformationSystems.API.Models
{
    public class AuthenticateRequest
    {
        [Required]
        public string VAT { get; set; }

        [Required]
        public string Password { get; set; }
    }
}