using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto;
using Enrich.Dto.Attributes;
using Enrich.Dto.Parameters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.Data.Dapper
{
    public class GridSqlHelper
    {
        public static List<SqlMapDto> GetFields<TItem>()
        {
            var fields = new List<SqlMapDto>();
            var props = typeof(TItem).GetProperties();

            foreach (var property in props)
            {
                if (!property.TryGetAttribute<SqlMapDtoAttribute>(out var attr))
                {
                    continue;
                }

                var field = new SqlMapDto
                {
                    IsUseToSort = attr.UseToSort,
                    IsRequired = attr.IsRequired,
                    SqlName = attr.SqlName,
                    DtoName = !string.IsNullOrWhiteSpace(attr.DtoName) ? attr.DtoName : property.Name,
                    SqlJoinKeys = attr.SqlJoinKeys,
                    Config = property.GetAttribute<GridFieldAttribute>(),
                    IsTemplateField = attr.IsTemplateField
                };
                fields.Add(field);
            }

            //set Setting for fields are group
            //foreach (var group in fields.Where(a => !string.IsNullOrWhiteSpace(a.Config?.Group)).GroupBy(a => a.Config.Group))
            //{
            //    //get main setting from item has "ClientNamePrefix" not empty
            //    var mainSetting = group.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.Config.ClientPrefix))?.Config;
            //    if (mainSetting != null)
            //    {
            //        foreach (var item in group)
            //        {
            //            item.Config = mainSetting;
            //        }
            //    }
            //}
            return fields;
        }

        public static IEnumerable<SqlMapDto> GetDefaultGridFields<T>()
            => GetFields<T>().Where(a => a.Config != null && a.IsDefault);

        public static List<OrderByParameter> GetOrderFields(List<SqlMapDto> fields, string requestFields)
        {
            if (string.IsNullOrWhiteSpace(requestFields))
            {
                return new List<OrderByParameter>();
            }

            var orderFields = requestFields.SplitEx()
                .Select(name => new OrderByParameter(name.StartsWith("-") ? SortDirectionEnum.Descending : SortDirectionEnum.Ascending)
                {
                    Field = fields.FirstOrDefault(a => a.DtoName.EqualsEx(name.Trim('-')))?.Clone()
                })
                .Where(a => a.Field != null && a.Field.CanSort)
                .GroupBy(a => a.Field.SqlName)
                .Select(a => a.First()).ToList();

            return orderFields;
        }

        public static List<SqlMapDto> GetSelectFields(List<SqlMapDto> fields, string requestFields, Language? language, bool defaultIfNotFound = true)
        {
            var selectFields = new List<SqlMapDto>();

            var removeDtoFields = new List<string>();
            var requestDtoFields = requestFields.SplitEx().Select(a => a.Trim().ToUpper()).ToArray();

            if (requestDtoFields.Length > 0)
            {
                selectFields.AddRange(requestDtoFields.Contains("*")
                    ? fields.Where(a => a.Config != null).Select(a => a.Clone())
                    : fields.Where(a => requestDtoFields.Contains(a.DtoName.ToUpper())).Select(a => a.Clone()));
            }

            if (selectFields.Count == 0 && defaultIfNotFound)
            {
                selectFields.AddRange(fields.Where(a => a.Config != null && a.IsDefault).Select(a => a.Clone()));
            }

            //optimize: always include, remove field.IsUseToSort = true & add related fields (Setting.RelatedDtoNames)
            selectFields.AddRange(fields.Where(a => a.IsRequired).Select(a => a.Clone()));

            removeDtoFields.AddRange(selectFields.Where(a => !a.IsRequired && a.IsUseToSort).Select(a => a.DtoName));

            var relatedDtos = selectFields.Where(a => !string.IsNullOrWhiteSpace(a.Config?.RelatedDtoNames)).Select(a => new { a.DtoName, a.Config.RelatedDtoNames }).ToList();
            foreach (var relatedDto in relatedDtos)
            {
                var relatedDtoNames = relatedDto.RelatedDtoNames.SplitEx();
                selectFields.AddRange(fields.Where(a => relatedDtoNames.Contains(a.DtoName)).Select(a => a.Clone()));
            }


            if (removeDtoFields.Count > 0)
            {
                selectFields.RemoveAll(a => removeDtoFields.Contains(a.DtoName));
            }

            //always add Id field
            if (selectFields.All(a => !a.EqualsDtoName("Id")))
            {
                var fieldId = fields.FirstOrDefault(a => a.EqualsDtoName("Id"));
                if (fieldId != null) selectFields.Add(fieldId.Clone());
            }

            return selectFields.GroupBy(a => new { a.DtoName }).Select(a => a.First()).ToList();
        }

        #region Cache

        private static ConcurrentDictionary<string, object> _caches = new ConcurrentDictionary<string, object>();

        private static T GetOrAddFromCache<T>(string key, Func<T> add)
        {
            var formatKey = $"{nameof(GridSqlHelper)}_{key}".ToUpper();
            if (_caches.ContainsKey(formatKey))
            {
                return (T)_caches[formatKey];
            }

            var value = add();
            _caches.AddOrUpdate(formatKey, value, (existingId, existingValue) => existingValue);

            return value;
        }

        #endregion
    }
}
