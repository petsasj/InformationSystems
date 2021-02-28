using System.ComponentModel.DataAnnotations;

namespace InformationSystems.API.Models
{
    public class CompanyCreationModel
    {
        [Required]
        public string RegisteredName { get; set; }

        public string Address { get; set; }

        [Required]
        public bool IsProvider { get; set; }

        [Required]
        public string Vat { get; set; }

        public string CallbackUrl { get; set; }

        [Required]
        public bool ReceiveConflictNotification { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
