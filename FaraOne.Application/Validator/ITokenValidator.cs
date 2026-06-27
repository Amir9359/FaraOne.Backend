using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace FaraOne.Application.Validator
{
    public interface ITokenValidator
    {
        Task Execute(TokenValidatedContext context);
    }
    public class TokenValidator : ITokenValidator
    {
        private readonly UserRepository _userRepository;
        private readonly UserTokenRepository _userTokenRepository;

        public TokenValidator(UserRepository userRepository, UserTokenRepository userTokenRepository)
        {
            _userRepository = userRepository;
            _userTokenRepository = userTokenRepository;
        }

        public async Task Execute(TokenValidatedContext context)
        {
            var claimIdnetity = context.Principal.Identity as ClaimsIdentity;
            if (claimIdnetity?.Claims == null  || !claimIdnetity.Claims.Any())
            {
                context.Fail("Claim Not found ... ");
                return;
            }

            var userId = claimIdnetity.FindFirst("UserId").Value;
       
            var user = _userRepository.GetUser(userId);
            if (!user.IsActive)
            {
                context.Fail("User Not Active");
                return;
            }

            if (!(context.SecurityToken is JwtSecurityToken token)
                || !_userTokenRepository.CheckExistToken(token.RawData))
            {
                context.Fail("توکن در دیتابیس وجود ندارد.");
                return;
            }
        }

    }
}