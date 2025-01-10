using Enrich.Dto.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto
{
    public class SqlMapDto
    {
        public string SqlName { get; set; }

        public string DtoName { get; set; } = string.Empty;

        public string SqlJoinKeys { get; set; }

        public bool IsUseToSort { get; set; }

        public bool IsRequired { get; set; }

        #region Custom

        public bool CanSort => Config?.CanSort ?? true;

        public bool IsDefault => Config?.IsDefault ?? false;

        public GridFieldAttribute Config { get; set; }

        public bool IsTemplateField { get; set; }

        #endregion

        public SqlMapDto()
        {
        }

        public SqlMapDto(string sqlName)
            : this(sqlName, sqlName)
        {
        }

        public SqlMapDto(string sqlName, string dtoName) : this()
        {
            SqlName = sqlName;
            DtoName = dtoName;
        }

        //https://msdn.microsoft.com/en-us/library/system.object.memberwiseclone(v=vs.110).aspx
        public SqlMapDto Clone()
        {
            return (SqlMapDto)this.MemberwiseClone();
        }

        public bool EqualsDtoName(string dtoName)
            => DtoName.Equals(dtoName.Trim(), StringComparison.OrdinalIgnoreCase);

        public string SqlSelectField
            => !string.IsNullOrWhiteSpace(DtoName) ? $"{SqlName} AS {DtoName}" : SqlName;
    }
}
