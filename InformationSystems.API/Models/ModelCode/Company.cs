using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using IdGen;

namespace InformationSystems.API.Models
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
            var cryptoService = new SimpleCrypto.PBKDF2();
            var passwordHash = cryptoService.Compute(password, this.PasswordSalt);
            var verificationResult = (passwordHash == this.PasswordHash);
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
