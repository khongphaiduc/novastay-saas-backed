using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStay.Infrastructure.ServicesImple
{
    public class ProvideAccessToken : IProvideAccessToken
    {
        private HostContext _dbcontext;
        private IJwtTokenService _refreshTokenService;

        public ProvideAccessToken(IJwtTokenService jwtTokenService, HostContext hostContext)
        {
            _dbcontext = hostContext;
            _refreshTokenService = jwtTokenService;
        }

        public async Task<AccessTokenDto> GetAccessToken(string refreshtoken)
        {

            var hashRefreshToken = _refreshTokenService.HashRefreshToken(refreshtoken);

            var user = await _dbcontext.AccountRefreshTokens.Where(s => s.TokenHash == hashRefreshToken && s.RevokedAt == null && s.ExpiresAt > DateTime.UtcNow).Select(t => new InforUser
            {
                AccountId = t.AccountId,
                OrganizationId = t.Account.OwnedOrganizations.Select(o => o.Id).FirstOrDefault(),
                AccountType = t.Account.AccountType,
                UserName = t.Account.CustomerName,
                Phone = t.Account.Phone,
                Email = t.Account.Email,
            }).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            var Expiry = DateTime.UtcNow.AddMinutes(60);
            var newAccessToken = _refreshTokenService.CreateAccessToken(user.AccountId, user.OrganizationId, user.AccountType, user.UserName, user.Phone, user.Email, Expiry);

            return new AccessTokenDto
            {
                AccessToken = newAccessToken,
                AccessTokenExpiresAt = Expiry
            };
        }
    }
}
