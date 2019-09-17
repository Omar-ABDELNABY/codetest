using codetest.Models;
using codetest.Models.Auth;
using codetest.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace codetest.Business_Logic
{
    public static class AuthBL
    {

        public static ICollection<Login> GetAllLoginsSync(IAuthRepository authRepository)
        {
            return authRepository.GetAllSync();
        }

        public static void AddManyLoginsSync(List<Login> logins, IAuthRepository authRepository)
        {
            authRepository.AddManySync(logins);
        }
        public static Login GetLogin(string UserLogin, IAuthRepository authRepository)
        {
            return authRepository.GetSingleByExpression(x => x.UserLogin == UserLogin).Result;
        }

        public static LoginResponse AuthenticateUser(Login loginInput, IAuthRepository authRepository, IConfiguration configuration)
        {
            Login login = GetLogin(loginInput.UserLogin, authRepository);

            if(login == null)
                return new LoginResponse() { success = false };

            //to be replaced by checkPassword function later that should include hashing
            bool LoginResult = login.Password == loginInput.Password;   
            
            if (LoginResult==true)
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(JwtRegisteredClaimNames.Sid,login.Id),
                    new Claim(JwtRegisteredClaimNames.UniqueName, login.UserLogin),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
                List<string> roles = login.Roles;
                //This is a very important line -> it adds the ROLES in the PAYLOAD of the TOKEN :)
                if (roles != null)
                    claims.AddRange(roles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("AuthParams")["JWTKey"]));
                var token = new JwtSecurityToken(
                    issuer: configuration.GetSection("AuthParams")["issuer"],
                    audience: configuration.GetSection("AuthParams")["audience"],
                    expires: DateTime.UtcNow.AddMinutes(Int32.Parse(configuration.GetSection("AuthParams")["expiryInMinutes"])),
                    claims: claims,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                    ); 
                return new LoginResponse()
                {
                    success = true,
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    claims = claims.ToArray(),
                    RememberMe = (login.RememberMe) ?? false
                };
            }
            return new LoginResponse() { success = false };

        }
    }
}
