using Dapper;
using Enrich.Common;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Member;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class MemberRepository : DapperGenericRepository<Member>, IMemberRepository
    {
        public MemberRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        public async Task<Member> GetMemberByEmailAndPassword(string email, string password)
        {
            var query = SqlScript.Member.GetMemberByEmailAndPassword;
            var parameters = new DynamicParameters();
            parameters.Add("PersonalEmail", email);
            parameters.Add("Password", password);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Member>(query, parameters);
            }
        }

        public async Task<Member> GetMemberBySecretKey(string secretKey)
        {
            var query = SqlScript.Member.GetMemberBySecretKey;
            var parameters = new DynamicParameters();
            parameters.Add("secretKey", secretKey);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Member>(query, parameters);
            }
        }

        public async Task<IEnumerable<MemberOptionItemDto>> GetSalesMemberAsync()
        {
            var query = SqlScript.Member.GetSalesMember;
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<MemberOptionItemDto>(query);
            }
        }

        public async Task<Member> GetByEmailAsync(string email)
        {
            var query = SqlScript.Member.GetByEmail;
            var parameters = new DynamicParameters();
            parameters.Add("PersonalEmail", email);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Member>(query, parameters);
            }
        }
    }
}
