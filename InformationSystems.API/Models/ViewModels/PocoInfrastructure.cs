using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InformationSystems.API.Models.ViewModels
{
    public class PocoInfrastructure
    {
        public string InfrastructureType { get; set; }

        public List<PocoGeoLocation> GeoLocations { get; set; } = new List<PocoGeoLocation>();
    }
}
