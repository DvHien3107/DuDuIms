using Enrich.Common;
using Enrich.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Data.Dapper
{
    public partial class DapperRepository
    {
        protected void AddConditionInList(List<string> conditions, string fieldName, int[] ids)
        {
            if (ids.IsNullOrEmpty())
            {
                return;
            }

            conditions.Add(ids.Length == 1 ? $"{fieldName} = {ids[0]}" : $"{fieldName} IN ({ids.ToStringList()})");
        }

        protected void AddConditionNotInList(List<string> conditions, string fieldName, int[] ids)
        {
            if (ids.IsNullOrEmpty())
            {
                return;
            }

            conditions.Add(ids.Length == 1 ? $"{fieldName} <> {ids[0]}" : $"{fieldName} NOT IN ({ids.ToStringList()})");
        }
        protected void AddConditionInList(List<string> conditions, string fieldName, List<string> values)
        {
            if (values.Count == 0)
            {
                return;
            }

            conditions.Add($"{fieldName} IN ({values.ToStringListWithSpecial()})");
        }

        protected void AddConditionNotInList(List<string> conditions, string fieldName, List<string> values)
        {
            if (values.Count == 0)
            {
                return;
            }

            conditions.Add($"{fieldName} NOT IN ({values.ToStringListWithSpecial()})");
        }

        protected void AddConditionWithSearchText(List<string> conditions, List<string> fieldsNames, string text = "")
        {
            if (fieldsNames.Count() == 0)
            {
                return;
            }
            var cons = new List<string>();
            foreach (var filed in fieldsNames)
            {
                cons.Add($" {filed} LIKE '%{text}%' ");
            }

            conditions.Add($"({string.Join("OR", cons)})");
        }

        protected void AddConditionWithFullTextSearch(List<string> conditions, string fieldsName = "*", string text = "")
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            conditions.Add($"CONTAINS(*, '\"*{text}*\"')");
        }

        protected bool AddConditionRangePrice(List<string> conditions, string fieldName, decimal? from, decimal? to)
        {
            if ((from == null || from == 0) && (to == null || to == 0))
            {
                return false;
            }

            if (from > 0 && to > 0)
            {
                conditions.Add($"{fieldName} BETWEEN {from.Value.SqlVal()} AND {to.Value.SqlVal()}");
            }
            else if (from > 0)
            {
                conditions.Add($"{fieldName} >= {from.Value.SqlVal()}");
            }
            else
            {
                conditions.Add($"{fieldName} <= {to.Value.SqlVal()}");
            }

            return true;
        }

        protected bool AddConditionRangeDate(List<string> conditions, string fieldName, DateTime? from, DateTime? to, string extendCondition = "")
        {
            var condition = GetConditionRangeDate(fieldName, from, to);

            if (!string.IsNullOrWhiteSpace(condition))
            {
                conditions.Add(string.IsNullOrWhiteSpace(extendCondition) ? condition : $"({condition} AND {extendCondition})");
                return true;
            }

            return false;
        }

        protected bool AddConditionRangeDateTime(List<string> conditions, string fieldName, DateTime? from, DateTime? to, string extendCondition = "")
        {
            var condition = GetConditionRangeDateTime(fieldName, from, to);

            if (!string.IsNullOrWhiteSpace(condition))
            {
                conditions.Add(string.IsNullOrWhiteSpace(extendCondition) ? condition : $"({condition} AND {extendCondition})");
                return true;
            }

            return false;
        }

        protected string GetConditionRangeDate(string fieldName, DateTime? from, DateTime? to)
        {
            if (from == null && to == null)
            {
                return string.Empty;
            }

            var conditionFrom = from != null ? $"{fieldName} >= {from.Value.SqlVal()}" : string.Empty;
            var conditionTo = string.Empty;

            if (to != null)
            {
                var tmp = to.Value.AddDays(1);
                conditionTo = $"{fieldName} < {tmp.SqlVal()}";
            }

            if (!string.IsNullOrWhiteSpace(conditionFrom) && !string.IsNullOrWhiteSpace(conditionTo))
            {
                return $"({conditionFrom} AND {conditionTo})";
            }

            if (!string.IsNullOrWhiteSpace(conditionFrom))
            {
                return conditionFrom;
            }

            return conditionTo;
        }

        protected string GetConditionRangeDateTime(string fieldName, DateTime? from, DateTime? to)
        {
            if (from == null && to == null)
            {
                return string.Empty;
            }

            var conditionFrom = from != null ? $"{fieldName} >= {from.Value.SqlVal()}" : string.Empty;
            var conditionTo = to != null ? $"{fieldName} <= {to.Value.SqlVal()}" : string.Empty;
           
            if (!string.IsNullOrWhiteSpace(conditionFrom) && !string.IsNullOrWhiteSpace(conditionTo))
            {
                return $"({conditionFrom} AND {conditionTo})";
            }

            if (!string.IsNullOrWhiteSpace(conditionFrom))
            {
                return conditionFrom;
            }

            return conditionTo;
        }
    }
}
