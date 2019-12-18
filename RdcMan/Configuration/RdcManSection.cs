using System.Configuration;

namespace RdcMan.Configuration
{
	public class RdcManSection : ConfigurationSection
	{
		[ConfigurationProperty("displaySizes", IsDefaultCollection = false)]
		public DisplaySizeElementCollection DisplaySizes => (DisplaySizeElementCollection)base["displaySizes"];

		[ConfigurationProperty("programUpdate")]
		public ProgramUpdateElement ProgramUpdate => (ProgramUpdateElement)base["programUpdate"];

		[ConfigurationProperty("warningThresholds")]
		public WarningThresholdsElement WarningThresholds => (WarningThresholdsElement)base["warningThresholds"];

		[ConfigurationProperty("logging")]
		public LoggingElement Logging => (LoggingElement)base["logging"];
	}
}
