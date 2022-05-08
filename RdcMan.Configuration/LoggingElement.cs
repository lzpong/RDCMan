using System.Configuration;

namespace RdcMan.Configuration {
	public class LoggingElement : ConfigurationElement {
		[ConfigurationProperty("enabled")]
		public bool Enabled {
			get {
				return (bool)base["enabled"];
			}
			set {
				base["enabled"] = value;
			}
		}

		[ConfigurationProperty("path")]
		public string Path {
			get {
				return (string)base["path"];
			}
			set {
				base["path"] = value;
			}
		}

		[ConfigurationProperty("maximumNumberOfFiles", DefaultValue = 5)]
		public int MaximumNumberOfFiles {
			get {
				return (int)base["maximumNumberOfFiles"];
			}
			set {
				base["maximumNumberOfFiles"] = value;
			}
		}
	}
}
