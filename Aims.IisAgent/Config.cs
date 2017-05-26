using System;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace Aims.IISAgent
{
    public static class Config
    {
        public static string ApiEndPoint
        {
            get { return ConfigurationManager.AppSettings["api-endpoint"]; }
        }

        public static Guid EnvironmentId
        {
            get
            {
                Guid value;
                if (!Guid.TryParse(ConfigurationManager.AppSettings["environment-id"], out value))
                    throw new FormatException("'environment-id' setting has invalid format.");
                return value;
            }
        }

        public static string Token
        {
            get { return ConfigurationManager.AppSettings["token"]; }
        }

        public static bool VerboseLog
        {
            get
            {
                bool value;
                return Boolean.TryParse(ConfigurationManager.AppSettings["verbose-log"], out value) && value;
            }
        }
    }
}