﻿using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace InformationSystems.API.Models
{

    public partial class InfrastructureModificationRequest
    {
        public InfrastructureModificationRequest(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            DateCreated = DateTime.UtcNow;
        }
    }

}
