using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace InformationSystems.API
{

    public partial class Infrastructure
    {
        public Infrastructure(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        public bool IsApproved => this.ModificationRequest.Approved ?? false;
    }

}
