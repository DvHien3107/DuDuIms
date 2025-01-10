using Dapper;
using Microsoft.IdentityModel.Tokens;
using Pos.Application.DBContext;
using Pos.Application.Extensions.Helper;
using Pos.Application.Repository;
using Pos.Model.Model.Auth;
using Pos.Model.Model.Table;
using Pos.Model.Model.Table.IMS;
using Pos.Model.Model.Table.POS;
using Pos.Model.Model.Table.RCP;
using Promotion.Application.Extensions;
using Promotion.Application.Interfaces;
using Promotion.Model.Model;
using Promotion.Model.Model.Comon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Twilio.Jwt.AccessToken;

namespace Pos.Application.Services.Scoped
{
    public interface ILoginService : IEntityService<Store>
    {
        Task<P_Member?> doLogin(string email, string pass);
        string LoginUser(string UserID, string Password);
        IEnumerable<dynamic> getJWTTokenClaim(string token);
        int getStoreIdByToken(string token); 
        Task<Store> StoreLogin(string email, string pass);
    }
    public class LoginService : RCPEntityService<Store>, ILoginService
    {
        private ILoginRepository _loginRepository;
        private IRcpRepository _rcpRepository;
        public LoginService(ILoginRepository loginRepository, IRcpRepository rcpRepository)
        {
            _loginRepository = loginRepository;
            _rcpRepository = rcpRepository;
        }
        public LoginService(IDbConnection db) : base(db)
        {

        }
        public async Task<Store> StoreLogin(string email, string pass)
        {
            string passEncrypt = EncryptionHelper.Md5Hash(pass).ToUpper();
            IEnumerable<Store> lstStore = await _rcpRepository.getStoreByLogin(email);
            Store result = lstStore.FirstOrDefault(x => x.Password == passEncrypt);
            return result;
        }
        public async Task<P_Member?> doLogin(string email, string pass)
        {
            //string passEncrypt = EncryptHelper.Md5Hash(pass).ToUpper();
            return await _loginRepository.doLogin(email, pass);
        }
        private IEnumerable<Store> getListStore(string where)
        {
            return _connection.AutoConnect().SqlQuery<Store>($"select * from FT_Store() Where {where}");
        }


        private IEnumerable<Claim> GetUserClaims(JWTUser user)
        {
            IEnumerable<Claim> claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, user.STORE_NAME),
                new Claim("USERID", user.USERID),
                new Claim("EMAILID", user.EMAILID),
                new Claim("PHONE", user.PHONE),
                new Claim("ACCESS_LEVEL", user.ACCESS_LEVEL.ToUpper()),
                new Claim("READ_ONLY", user.READ_ONLY.ToUpper())
            };
            return claims;
        }

        public IEnumerable<dynamic> getJWTTokenClaim(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                var claimValue = securityToken.Claims.Select(x => new { x.Value, x.Type });
                return claimValue;
            }
            catch (Exception)
            {
                //TODO: Logger.Error
                return null;
            }
        }

        public string LoginUser(string UserID, string Password)
        {
            //Get user details for the user who is trying to login
            var LstUser = getListStore($" LOWER(Email) = LOWER('{UserID}')");
            var user = LstUser.FirstOrDefault();

            //Authenticate User, Check if it’s a registered user in Database
            if (user == null)
                return null;

            //If it's registered user, check user password stored in Database 
            //For demo, password is not hashed. Simple string comparison 
            //In real, password would be hashed and stored in DB. Before comparing, hash the password
            Password = Password.CreateMD5Hash();
            if (Password.ToLower() == user.Password.ToLower())
            {
                //Authentication successful, Issue Token with user credentials
                //Provide the security key which was given in the JWToken configuration in Startup.cs
                var key = Encoding.ASCII.GetBytes(AuthData.Key);
                //Generate Token for user 
                var jwtuser = new JWTUser
                {
                    USERID = user.StoreID.ToString(),
                    PASSWORD = user.Password,
                    STORE_NAME = user.StoreName,
                    EMAILID = user.Email,
                    PHONE = user.Phone,
                    ACCESS_LEVEL = "Analyst",
                    READ_ONLY = "true"
                };

                var JWToken = new JwtSecurityToken(
                    issuer: AuthData.Issuer,
                    audience: AuthData.Audience,
                    claims: GetUserClaims(jwtuser),
                    notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                    expires: new DateTimeOffset(DateTime.Now.AddDays(1)).DateTime,
                    //Using HS256 Algorithm to encrypt Token
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key),
                                        SecurityAlgorithms.HmacSha256Signature)
                );
                var token = new JwtSecurityTokenHandler().WriteToken(JWToken);
                return token;
            }
            else
            {
                return null;
            }
        }

        public int getStoreIdByToken(string token)
        {
            token = token.Replace("Bearer ", "");
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
            //var claimValue = securityToken.Claims.Select(x => new { x.Value, x.Type });
            var StoreID = securityToken.Claims.FirstOrDefault(x => x.Type == "USERID").Value;
            return Convert.ToInt32(StoreID);
        }
    }
}
