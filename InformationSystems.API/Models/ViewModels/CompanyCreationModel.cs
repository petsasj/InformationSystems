using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace InformationSystems.API.Models.ViewModels
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
