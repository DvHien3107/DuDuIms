using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Implement.Library
{
    public static class LibraryHelper
    {     

        public static T CloneByJson<T>(this T source, Action<T> afterCloned = null, Action<Exception> errorCloned = null) where T : class
        {
            if (source == null)
            {
                return null;
            }

            T clone = default(T);

            try
            {
                var json = JsonConvert.SerializeObject(source);
                clone = JsonConvert.DeserializeObject<T>(json);

                afterCloned?.Invoke(clone);
            }
            catch (Exception ex)
            {
                errorCloned?.Invoke(ex);
            }

            return clone;
        }

        public static string ConvertColorArgbToHtml(this int colorArgb)
        {
            try
            {
                return ColorTranslator.ToHtml(Color.FromArgb(colorArgb));
            }
            catch
            {
                return string.Empty;
            }
        }
      
    }
}
