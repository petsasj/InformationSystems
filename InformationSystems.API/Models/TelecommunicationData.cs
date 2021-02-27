using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InformationSystems.API.Models
{
    public class TelecommunicationDataRequest
    {
        [JsonRequired]
        public int InternalId { get; set; }

        public string CallBackUrl { get; set; }

        [JsonRequired]
        public string InfrastructureType { get;set; }

        public JObject Data { get; set; }
    }
}

