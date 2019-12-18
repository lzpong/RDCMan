using System.Configuration;

namespace RdcMan.Configuration
{
	public class DisplaySizeElement : ConfigurationElement
	{
		[ConfigurationProperty("size")]
		public string Size
		{
			get
			{
				return (string)base["size"];
			}
			set
			{
				base["size"] = value.ToString();
			}
		}
	}
}
