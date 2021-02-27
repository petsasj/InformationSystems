namespace InformationSystems.API.Models
{
    public class AuthenticateResponse
    {
        public long Id { get; set; }
        public string RegisteredName { get; set; }
        public string VatNormalized { get; set; }
        public string Token { get; set; }

        public AuthenticateResponse(Company company, string token)
        {
            Id = company.InternalId;
            RegisteredName = company.RegisteredName;
            VatNormalized = company.VatNormalized;
            Token = token;
        }
    }
}