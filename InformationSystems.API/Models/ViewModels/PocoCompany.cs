using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InformationSystems.API.Models.ViewModels
{
    /// <summary>
    /// Concise class containing only necessary properties of Company class
    /// Used for caching purposes
    /// </summary>
    public class PocoCompany
    {
        public string RegisteredName { get; set; }

        public long InternalId { get; set; }

        public bool IsProvider { get; set; }

        public bool IsActive { get; set; }

        public PocoCompany(Company company)
        {
            this.RegisteredName = company.RegisteredName;
            this.InternalId = company.InternalId;
            this.IsActive = company.IsActive;
            this.IsProvider = company.IsProvider;
        }
    }
}
