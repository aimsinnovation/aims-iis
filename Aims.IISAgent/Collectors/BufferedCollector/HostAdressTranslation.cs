using System.Text.RegularExpressions;

namespace Aims.IISAgent.Collectors.BufferedCollector
{
	public static class HostAdressTranslation
	{
		//convert localhost and ip adresses to string.Empty
		//ip adress doesn't have validation, I trust people, that develop IIS
		//and this comment will help peple that will use this code in future
		public static string ReplaceIfLocalhostOrIp(this string hostAdress)
		{
			var regex = new Regex("(localhost)|(([0-9]{1,3}\\.){3}[0-9]{1,3})");//find "localhost" or some ip adress
			return regex.Replace(hostAdress, string.Empty);
		}

		public static string EraseSlash(this string hostAdress)
		{
			var regex1 = new Regex("\\\\");//wolverine
			var regex2 = new Regex("/");//toothpick
			return regex2.Replace(regex1.Replace(hostAdress, string.Empty), string.Empty);
		}
	}
}