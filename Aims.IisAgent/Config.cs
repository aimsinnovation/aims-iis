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
		//<add key = "topology-update-period" value="5"/>
		//<add key = "statistic-collect-period" value="1"/>

	    //time in minutes
		public static double TopologyUpdatePeriod
	    {
		    get
		    {
			    double value;
				if(!Double.TryParse(ConfigurationManager.AppSettings["topology-update-period"], out value))
				    throw new FormatException("'topology-update-period' setting has invalid format.");
				if(value <= 0.0)
					throw new FormatException("'topology-update-period' must be positive.");
			    return value;
			}
	    }

	    //time in minutes
	    public static double StatisticCollectPeriod
	    {
		    get
		    {
			    double value;
			    if(!Double.TryParse(ConfigurationManager.AppSettings["statistic-collect-period"], out value))
				    throw new FormatException("'statistic-collect-period' setting has invalid format.");
			    if(value <= 0.0)
				    throw new FormatException("'statistic-collect-period' must be positive.");
				return value;
		    }
	    }

	    //time in months
		public static double SslCertFirstWarning
	    {
		    get
		    {
			    double value;
			    if (!Double.TryParse(ConfigurationManager.AppSettings["ssl-cert-warning-first"], out value))
				    throw new FormatException("'ssl-cert-warning-first' setting has invalid format.");
			    if (value <= 0.0)
				    throw new FormatException("'ssl-cert-warning-first' must be positive.");
			    if (value <= SslCertSecondWarning)
				    throw new FormatException("'ssl-cert-warning-first' must be more then 'ssl-cert-warning-second'.");
				return value;
		    }
	    }

		//time in months
		public static double SslCertSecondWarning
		{
		    get
		    {
			    double value;
			    if (!Double.TryParse(ConfigurationManager.AppSettings["ssl-cert-warning-second"], out value))
				    throw new FormatException("'ssl-cert-warning-second' setting has invalid format.");
			    if (value <= 0.0)
				    throw new FormatException("'ssl-cert-warning-second' must be positive.");
			    return value;
			}
	    }

	}
}