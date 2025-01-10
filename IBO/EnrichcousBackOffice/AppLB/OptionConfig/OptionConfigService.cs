using EnrichcousBackOffice.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace EnrichcousBackOffice.AppLB.OptionConfig
{
    public partial class OptionConfigService
    {
        private readonly WebDataModel db = new WebDataModel();
        #region Utilities 
    
        protected virtual IDictionary<string, IList<Option_Config>> GetAllOptionConfig()
        {
            var query = from s in db.Option_Config
                        orderby s.Key
                        select s;
            var settings = query.ToList();
            var dictionary = new Dictionary<string, IList<Option_Config>>();
            foreach (var s in settings)
            {
                var resourceName = s.Key.ToLowerInvariant();
                var settingForCaching = new Option_Config
                {
                    Key = s.Key,
                    Value = s.Value,
                };
                if (!dictionary.ContainsKey(resourceName))
                {
                    //first setting
                    dictionary.Add(resourceName, new List<Option_Config>
                        {
                            settingForCaching
                     });
                }
                else
                {
                    //already added
                    //most probably it's the setting with the same name but for some certain store (storeId > 0)
                    dictionary[resourceName].Add(settingForCaching);
                }
            }
            return dictionary;
        }
        protected virtual void SetOptionConfig(Type type, string key, object value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            key = key.Trim().ToLowerInvariant();
            var valueStr = TypeDescriptor.GetConverter(type).ConvertToInvariantString(value);

            var allSettings = GetAllOptionConfig();
            var settings = allSettings.ContainsKey(key) ?
                allSettings[key].FirstOrDefault() : null;
            if (settings != null)
            {
                //update
                var setting = GetSettingByKey(settings.Key);
                setting.Value = valueStr;
                UpdateOptionConfig(setting);
            }
            else
            {
                //insert
                var optionconfig = new Option_Config
                {
                    Key = key,
                    Value = valueStr
                };
                InsertOptionConfig(optionconfig);
            }
        }

        #endregion
        public virtual Option_Config GetSettingByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            return db.Option_Config.FirstOrDefault(x => x.Key == key);
        }

        public virtual void InsertOptionConfig(Option_Config option, bool clearCache = true)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));
        }

        /// <summary>
        /// Updates a setting
        /// </summary>
        /// <param name="setting">Setting</param>
        /// <param name="clearCache">A value indicating whether to clear cache after setting update</param>
        public virtual void UpdateOptionConfig(Option_Config setting)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            db.SaveChanges();
        }

        /// <summary>
        /// Deletes a setting
        /// </summary>
        /// <param name="setting">Setting</param>
        public virtual void DeleteOption_Config(Option_Config option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            db.Option_Config.Remove(option);
            db.SaveChanges();
        }

      
        public T GetSettingByKey<T>(string key, T defaultValue = default)
        {
            var query = from s in db.Option_Config where s.Key == key select s;
            var setting = query.FirstOrDefault();
            return setting.Value != null ? CommonHelper.To<T>(setting.Value) : defaultValue;
        }

        public virtual T LoadSetting<T>() where T : IOptionConfig, new()
        {
            return (T)LoadOption_Config(typeof(T));
        }

        /// <summary>
        /// Load settings
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="storeId">Store identifier for which settings should be loaded</param>
        public virtual IOptionConfig LoadOption_Config(Type type)
        {
            var options = Activator.CreateInstance(type);

            foreach (var prop in type.GetProperties())
            {
                // get properties we can read and write to
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                var key = type.Name + "." + prop.Name;
                //load by store
                var setting = GetSettingByKey<string>(key);
                if (setting == null)
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).IsValid(setting))
                    continue;

                var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(setting);

                //set property
                prop.SetValue(options, value, null);
            }

            return options as IOptionConfig;
        }
        public virtual void SetOptionConfig<T>(string key, T value)
        {
            SetOptionConfig(typeof(T), key, value);
        }
        public virtual void SaveSetting<T>(T settings) where T : IOptionConfig, new()
        {
            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            foreach (var prop in typeof(T).GetProperties())
            {
                // get properties we can read and write to
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                if (!TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    continue;

                var key = typeof(T).Name + "." + prop.Name;
                var value = prop.GetValue(settings, null);
                if (value != null)
                    SetOptionConfig(prop.PropertyType, key, value);
                else
                    SetOptionConfig(key, string.Empty);
            }

        }


        public virtual void SaveOptionConfig<T, TPropType>(T options,
            Expression<Func<T, TPropType>> keySelector) where T : IOptionConfig, new()
        {
            if (!(keySelector.Body is MemberExpression member))
            {
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    keySelector));
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException(string.Format(
                       "Expression '{0}' refers to a field, not a property.",
                       keySelector));
            }

            var key = GetOptionConfigKey(options, keySelector);
            var value = (TPropType)propInfo.GetValue(options, null);
            if (value != null)
                SetOptionConfig(key, value);
            else
                SetOptionConfig(key, string.Empty);
        }
        public virtual string GetOptionConfigKey<TSettings, T>(TSettings settings, Expression<Func<TSettings, T>> keySelector)
            where TSettings : IOptionConfig, new()
        {
            if (!(keySelector.Body is MemberExpression member))
                throw new ArgumentException($"Expression '{keySelector}' refers to a method, not a property.");

            if (!(member.Member is PropertyInfo propInfo))
                throw new ArgumentException($"Expression '{keySelector}' refers to a field, not a property.");

            var key = $"{typeof(TSettings).Name}.{propInfo.Name}";

            return key;
        }

    }
}