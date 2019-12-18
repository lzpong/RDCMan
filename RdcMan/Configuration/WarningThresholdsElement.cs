using System.Configuration;

namespace RdcMan.Configuration
{
	public class WarningThresholdsElement : ConfigurationElement
	{
		[ConfigurationProperty("connect", DefaultValue = 10)]
		public int Connect
		{
			get
			{
				return (int)base["connect"];
			}
			set
			{
				base["connect"] = value;
			}
		}
	}
}
