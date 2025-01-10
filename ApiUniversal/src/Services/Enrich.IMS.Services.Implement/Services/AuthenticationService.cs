using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Authentication;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public class AuthenticationService : GenericService<Member, MemberDto>, IAuthenticationService
    {
        private readonly IMemberRepository _repository;
        private readonly JwtSettings _jwtSettings;
        private readonly EnrichContext _context;
        public AuthenticationService(IMemberRepository repository, IMemberMapper mapper, JwtSettings jwtSettings, EnrichContext context)
            : base(repository, mapper)
        {
            _repository = repository;
            _jwtSettings = jwtSettings;
            _context = context;
        }
        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {

            var user = await _repository.GetMemberByEmailAndPassword(request.Email, request.Password);
            if (user == null)
            {
                throw new Exception($"User with {request.Email} not found.");
            }

            JwtSecurityToken jwtSecurityToken = await GenerateToken(user);

            AuthenticationResponse response = new AuthenticationResponse()
            {
                Id = user.Id,
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Email = user.PersonalEmail,
                ExpiresIn = _jwtSettings.DurationInMinutes,
                RefreshToken = GenerateRefreshToken()
            };
            return response;
        }

        /// <summary>
        /// get new token from refresh token
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<AuthenticationResponse> RefreshTokenAsync()
        {
            var user = await _repository.FindByIdAsync(_context.UserId);
            if (user == null)
            {
                throw new Exception($"User with Id{_context.UserId} not found.");
            }

            JwtSecurityToken jwtSecurityToken = await GenerateToken(user);

            AuthenticationResponse response = new AuthenticationResponse()
            {
                Id = user.Id,
                AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Email = user.PersonalEmail,
                ExpiresIn = _jwtSettings.DurationInMinutes,
                RefreshToken = GenerateRefreshToken()
            };
            return response;
        }

        /// <summary>
        /// generate new token base on member
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        private async Task<JwtSecurityToken> GenerateToken(Member member)
        {
            //var roleClaims = new List<Claim>();    
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signinCredential = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
               {
                    new Claim(JwtRegisteredClaimNames.Email, member.PersonalEmail),
                    new Claim("uid", member.Id.ToString())
                };

            var jwtSecurityToken = new JwtSecurityToken(
                claims: claims,
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: DateTime.Now.AddSeconds(_jwtSettings.DurationInMinutes),
                signingCredentials: signinCredential
                );
            return jwtSecurityToken;
        }
        

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<UserProfile> GetOauth2UserProfileAsync(AuthenticationHeaderValue Authentication, (string Key, string Value)[] extends)
        {
            var accessToken = extends.FirstOrDefault(a => a.Key == "Parameter").Value ?? string.Empty;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);
            try
            {
                tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                
                var memberId = int.Parse(jwtToken.Claims.First(x => x.Type == "uid").Value);
                var user = await _repository.FindByIdAsync(memberId);
                var result = new UserProfile()
                {
                    UserId = memberId,
                    PersonalEmail = user.PersonalEmail
                };
                return result;
            }
            catch (SecurityTokenExpiredException ex)
            {
                throw ex;
            }       
        }
    }
}
