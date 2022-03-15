using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider.Common
{
    public class CommonSetting
    {
        private static T Setting<T>(string name)
        {
            string value = ConfigurationManager.AppSettings[name];

            if (value == null)
                throw new Exception(String.Format("Could not find setting '{0}',", name));

            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public static string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["defaultConnection"].ConnectionString;
            }
        }
    }
}
