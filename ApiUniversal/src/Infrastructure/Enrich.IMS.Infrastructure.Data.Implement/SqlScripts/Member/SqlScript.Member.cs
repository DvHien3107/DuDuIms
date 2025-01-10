using Enrich.Common.Enums;
using Enrich.IMS.Dto;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class Member
        {
            private const string Alias = SqlTables.Member;

            public const string GetByEmail = $"SELECT TOP(1) * FROM {Alias} WITH (NOLOCK) WHERE PersonalEmail = @PersonalEmail and ISNULL(Active,1)=1 AND ISNULL([Delete],0)!=1";

            public const string GetMemberBySecretKey = $"SELECT TOP(1) p.* FROM MemberAccessKey a inner join P_Member p on a.MemberId = p.Id where secretKey = @secretKey and ISNULL(IsActive,1)=1 AND DeletedDate is null";

            public const string GetMemberByEmailAndPassword = $"SELECT TOP(1) * FROM {Alias} WITH (NOLOCK) WHERE PersonalEmail = @PersonalEmail and Password = @Password and ISNULL(Active,1)=1 AND ISNULL([Delete],0)!=1";

            public const string GetSalesMember = $"SELECT MemberNumber AS Value, [FullName] AS Name FROM {Alias} WHERE Active = 1 AND CHARINDEX('19120010', DepartmentId) > 0";
        }
    }
}