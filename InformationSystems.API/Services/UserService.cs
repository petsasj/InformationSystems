using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpo;
using InformationSystems.API.Models;
using InformationSystems.API.Models.ViewModels;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InformationSystems.API.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticateResponse> Authenticate(AuthenticateRequest model);
        PocoCompany GetById(long internalId);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppSettings _appSettings;
        private readonly UnitOfWork _unitOfWork;

        public AuthenticationService(IOptions<AppSettings> appSettings,
            UnitOfWork unitOfWork)
        {
            this._appSettings = appSettings.Value;
            this._unitOfWork = unitOfWork;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest model)
        {
            var company = await _unitOfWork.Query<Company>()
                .SingleOrDefaultAsync(c => c.Vat.ToLower() == model.VAT.ToLower());

            // Company was not found, or invalid password
            if (company == null || !company.VerifyPassword(model.Password))
                return null;

            // authentication successful so generate jwt token
            var token = GenerateJwtToken(company);

            return new AuthenticateResponse(company, token);
        }

        public PocoCompany GetById(long internalId)
        {
            return _unitOfWork.Query<Company>()
                .Where(c => c.InternalId == internalId)
                .Select(c => new PocoCompany(c))
                .SingleOrDefault();
        }

        // helper methods

        private string GenerateJwtToken(Company company)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", company.InternalId.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}