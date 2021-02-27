﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace InformationSystems.API
{

    public partial class Infrastructure : XPObject
    {
        Company fCompany;
        [Association(@"InfrastructureReferencesCompany")]
        public Company Company
        {
            get { return fCompany; }
            set { SetPropertyValue<Company>(nameof(Company), ref fCompany, value); }
        }
        string fInfrastructureType;
        public string InfrastructureType
        {
            get { return fInfrastructureType; }
            set { SetPropertyValue<string>(nameof(InfrastructureType), ref fInfrastructureType, value); }
        }
        string fGeoJSONType;
        public string GeoJSONType
        {
            get { return fGeoJSONType; }
            set { SetPropertyValue<string>(nameof(GeoJSONType), ref fGeoJSONType, value); }
        }
        InfrastructureModificationRequest fModificationRequest;
        [Association(@"InfrastructureReferencesInfrastructureModificationRequest")]
        public InfrastructureModificationRequest ModificationRequest
        {
            get { return fModificationRequest; }
            set { SetPropertyValue<InfrastructureModificationRequest>(nameof(ModificationRequest), ref fModificationRequest, value); }
        }
        string fDateCreated;
        public string DateCreated
        {
            get { return fDateCreated; }
            set { SetPropertyValue<string>(nameof(DateCreated), ref fDateCreated, value); }
        }
        [Association(@"GeoLocationReferencesInfrastructure"), Aggregated]
        public XPCollection<GeoLocation> GeoLocations { get { return GetCollection<GeoLocation>(nameof(GeoLocations)); } }
    }

}
