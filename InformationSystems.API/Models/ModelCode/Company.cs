﻿using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using IdGen;

namespace InformationSystems.API
{
    public partial class Company
    {
        public Company(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction(); 
            DateCreated = DateTime.UtcNow;
            this.InternalId = new IdGenerator(0).CreateId();
        }

        public string VatNormalized => this.Vat?.ToLower();

        public bool VerifyPassword(string password)
        {
            var cryptoservice = new SimpleCrypto.PBKDF2();
            var pashwordHash = new SimpleCrypto.PBKDF2().Compute(password, this.PasswordSalt);
            var verificationResult = (pashwordHash == this.PasswordHash);
            return verificationResult;
        }

        public void SetPassword(string password)
        {
            var cryptoservice = new SimpleCrypto.PBKDF2();
            var hash = cryptoservice.Compute(password);
            var salt = cryptoservice.Salt;
            this.PasswordHash = hash;
            this.PasswordSalt = salt;
        }
    }

}
