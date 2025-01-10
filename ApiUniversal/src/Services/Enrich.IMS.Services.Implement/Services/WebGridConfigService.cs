using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.Dto.Attributes;
using Enrich.Dto.Base;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Customer;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Dto.SalesLead;
using Enrich.IMS.Dto.SalesLeadComment;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Dto.Ticket.Reponses;
using Enrich.IMS.Dto.WebGridConfig;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Implement.Library.Lookup;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public class WebGridConfigService : GenericService<WebGridConfig, WebGridConfigDto>, IWebGridConfigService
    {
        private readonly EnrichContext _context;
        private readonly IEnrichContainer _container;
        private IWebGridConfigRepository _repository => _repositoryGeneric as IWebGridConfigRepository;

        public WebGridConfigService(IWebGridConfigRepository repository, IWebGridConfigMapper mapper, EnrichContext context, IEnrichContainer container)
            : base(repository, mapper)
        {
            _context = context;
            _container = container;
        }

        #region Get

        public async Task<GridConfigDto> GetUserGridConfigAsync(GridType type, long userId, bool defaltIfNotFound = true)
        {
            var dbConfig = await GetDbConfigAsync(type, userId);

            if (dbConfig == null || dbConfig.Fields.Count == 0)
            {
                if (defaltIfNotFound)
                {
                    var result = GetDefaultConfig(type);
                    // add lookup type for columns config
                    await SyncDataConfigAsync(type, result);
                    return result;
                }
                return null;
            }

            var defConfig = GetDefaultConfig(type);
            SyncDataConfig(dbConfig, defConfig);

            // add lookup type for columns config
            await SyncDataConfigAsync(type, dbConfig);

            return dbConfig;
        }

        public async Task<GridConfigDto> GetFavFilterGridConfigAsync(GridType type, long userId, int favFilterId, bool defaltIfNotFound = false)
        {
            var dbConfig = await GetDbConfigAsync(type, userId);

            if (dbConfig == null || dbConfig.Fields.Count == 0)
            {
                return defaltIfNotFound ? GetDefaultConfig(type) : null;
            }

            var defConfig = GetDefaultConfig(type);
            SyncDataConfig(dbConfig, defConfig);

            // add lookup type for columns config
            await SyncDataConfigAsync(type, dbConfig);

            return dbConfig;
        }

        private void SyncDataConfig(GridConfigDto targetConfig, GridConfigDto defaultConfig)
        {
            var invalidFields = new List<string>();

            //mapping system fields
            foreach (var field in targetConfig.Fields)
            {
                var defField = defaultConfig.Fields.FirstOrDefault(a => a.Name == field.Name);
                if (defField != null)
                {
                    field.CanSort = defField.CanSort;
                    field.DataType = defField.DataType;
                    field.DisplayType = defField.Name == "Date" ? defField.DataType : defField.DisplayType; //UWA-132: Improve DisplayType to 'Datetime' of grid-config request
                    field.DisplayTypeFormat = defField.DisplayTypeFormat;
                    field.SearchName = defField.SearchName;
                }
                else //invalid field
                {
                    invalidFields.Add(field.Name);
                }
            }

            //remove invalid field. Ex: version1 use Name="F_name", version2 change to "FName" -> remove config "Name=F_name"
            if (invalidFields.Count > 0)
            {
                targetConfig.Fields.RemoveAll(a => invalidFields.Contains(a.Name));
            }

            //new fields
            foreach (var defField in defaultConfig.Fields.Where(a => targetConfig.Fields.All(b => b.Name != a.Name)))
            {
                var cloneField = defField.Clone();
                cloneField.IsShow = false;
                targetConfig.Fields.Add(cloneField);
            }
        }

        /// <summary>
        /// customer field for filter grid by column 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="gridConfig"></param>
        /// <returns></returns>
        private async Task SyncDataConfigAsync(GridType type, GridConfigDto gridConfig)
        {
            if (type == GridType.SearchTicket || type == GridType.SearchSalesLead || type == GridType.SearchMerchant)
            {
                var columnFilterManager = _container.ResolveByKeyed<LookupInfoManager>(type);
                var columns = await columnFilterManager.GetLookupConfigAsync(type);

                foreach (var gridField in gridConfig?.Fields)
                {
                    foreach (var lookupColumn in columns)
                    {
                        if (lookupColumn.Key == gridField.Name)
                        {
                            gridField.LookupInfo = lookupColumn.Value;
                        }
                    }
                }
            }
        }

        private GridConfigDto GetDefaultConfig(GridType type)
        {
            var config = new GridConfigDto();

            if (_defaultTypeSettings.TryGetValue(type, out var setting) && setting.Type != null)
            {
                config.Fields = GetGridFieldConfigs(setting.Type);

                //set default sort direction
                if (!string.IsNullOrWhiteSpace(setting.SortField))
                {
                    var defField = config.Fields.FirstOrDefault(a => a.Name.EqualsEx(setting.SortField));
                    if (defField != null)
                    {
                        defField.SortDirection = setting.SortDirection;
                    }
                }
            }
            return config;
        }

        private async Task<GridConfigDto> GetDbConfigAsync(GridType type, long userId)
        {
            var config = await _repository.GetUserGridConfigAsync(type, userId, includeGlobal: true);

            if (config == null)
            {
                return null;
            }

            var dto = JsonConvert.DeserializeObject<GridConfigDto>(config.ConfigAsJson);

            return dto;
        }

        #endregion

        #region Save

        public async Task<bool> SaveConfigAsync(SaveWebGridConfigRequest request)
        {
            //optimize: clear system fields
            var config = request.Config.Clone();

            foreach (var field in config.Fields)
            {
                field.CanSort = false;
                field.DataType = null;
                field.DisplayType = null;
                field.DisplayTypeFormat = null;
                field.SearchName = null;
            }

            //convert to json
            request.ConfigAsJson = JsonConvert.SerializeObject(config, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            request.UserId = _context.UserId;

            //save
            var configId = await _repository.SaveGridConfigAsync(request);

            return configId > 0;
        }

        #endregion

        #region GridFieldConfigs

        private List<GridFieldConfigDto> GetGridFieldConfigs(Type type)
        {
            var configFields = new List<GridFieldConfigDto>();

            var groupConfigProps = type.GetProperties()
                .Select(a => new
                {
                    Prop = a,
                    Config = a.GetAttribute<GridFieldAttribute>()
                })
                .Where(a => a.Config != null)
                .GroupBy(a => new { a.Config.Group });

            foreach (var groupConfig in groupConfigProps)
            {
                if (!string.IsNullOrWhiteSpace(groupConfig.Key.Group))
                {
                    //with config has GroupName -> only get 1
                    if (configFields.Any(a => a.Name.EqualsEx(groupConfig.Key.Group)))
                        continue;

                    var item = groupConfig.First();
                    AddGridFieldConfig(configFields, groupConfig.Key.Group, item.Prop, item.Config);
                }
                else
                {
                    foreach (var item in groupConfig)
                    {
                        AddGridFieldConfig(configFields, item.Prop.Name, item.Prop, item.Config);
                    }
                }
            }

            return configFields.OrderByDescending(a => a.IsShow).ThenBy(a => a.Index > 0 ? a.Index : 100000).ThenBy(a => a.Name).ToList();
        }

        private GridFieldConfigDto AddGridFieldConfig(List<GridFieldConfigDto> configFields, string fieldName, PropertyInfo prop, GridFieldAttribute setting)
        {
            var config = new GridFieldConfigDto
            {
                Name = fieldName,
                Index = setting.Index,
                ColumnName = setting.ColumnName ?? fieldName,
                IsShow = setting.IsShow,
                CanSort = setting.CanSort,
                CanSearch = setting.CanSearch,
                FixedAlign = setting.FixedAlign,
                DataType = setting.DataType,
                DisplayType = setting.DisplayType,
                DisplayTypeFormat = setting.DisplayFormat,
            };

            //DisplayType
            if (string.IsNullOrWhiteSpace(config.DisplayType))
            {
                var typeCode = prop.PropertyType.GetTypeCode();

                if (typeCode == TypeCode.DateTime)
                    config.DisplayType = ColDisplayType.DateTime;

                else if (typeCode == TypeCode.Boolean)
                    config.DisplayType = ColDisplayType.Boolean;

                else if (typeCode.IsDecimalType())
                    config.DisplayType = ColDisplayType.Currency;

                else if (typeCode.IsNumericType())
                    config.DisplayType = ColDisplayType.Number;

                else
                    config.DisplayType = ColDisplayType.Text;
            }


            //DataType
            if (string.IsNullOrWhiteSpace(config.DataType))
            {
                config.DataType = GetColDataType(config.DisplayType);
            }

            //Id -> set DisplayType = Text
            if (fieldName == "Id")
            {
                config.DisplayType = ColDisplayType.Text;
            }

            configFields.Add(config);

            return config;
        }

        private string GetColDataType(string displayType)
        {
            switch (displayType)
            {
                case ColDisplayType.Number:
                case ColDisplayType.Currency:
                    return ColDataType.Number;

                case ColDisplayType.Boolean:
                    return ColDataType.Boolean;

                case ColDisplayType.DateTime:
                case ColDisplayType.Date:
                case ColDisplayType.Time:
                    return ColDataType.DateTime;

                case ColDisplayType.Color:
                case ColDisplayType.Icon:
                case ColDisplayType.Picture:
                case ColDisplayType.Phone:
                case ColDisplayType.Email:
                case ColDisplayType.Text:
                default:
                    return ColDataType.Text;
            }
        }

        #endregion

        public void OptimizeConditionRequest(LookupDataRequest request, LookupDataType type)
        {
        }


        private static Dictionary<GridType, (Type Type, string SortField, SortDirectionEnum SortDirection)> _defaultTypeSettings = new Dictionary<GridType, (Type DtoType, string DefSortField, SortDirectionEnum DefSortDirection)>
        {
            [GridType.SearchTicket] = (typeof(TicketListItemDto), nameof(TicketListItemDto.Id), SortDirectionEnum.Descending),
            [GridType.SearchSalesLead] = (typeof(SalesLeadListItemDto), nameof(SalesLeadListItemDto.Id), SortDirectionEnum.Descending),
            [GridType.SearchSalesLeadComment] = (typeof(SalesLeadCommentItemDto), nameof(SalesLeadCommentItemDto.Id), SortDirectionEnum.Descending),
            [GridType.SearchMerchant] = (typeof(CustomerListItemDto), nameof(CustomerListItemDto.Id), SortDirectionEnum.Descending),
            [GridType.ReportActiveMerchant] = (typeof(CustomerReportListItemDto), nameof(CustomerReportListItemDto.StoreCode), SortDirectionEnum.Descending)
        };
    }
}
