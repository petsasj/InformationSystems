using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InformationSystems.API.Models.ViewModels;

namespace InformationSystems.API.Models
{
    public class QueryResults
    {
        public List<QueryResult> Results { get; set; } = new List<QueryResult>();
    }

    public class QueryResult
    {
        public string CompanyName { get; set; }

        public string CompanyVat { get; set; }

        public List<PocoInfrastructure> Infrastructures { get; set; } = new List<PocoInfrastructure>();
    }
}
