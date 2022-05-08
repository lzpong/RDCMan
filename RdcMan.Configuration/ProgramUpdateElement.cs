using System.Configuration;

namespace RdcMan.Configuration {
	public class ProgramUpdateElement : ConfigurationElement {
		[ConfigurationProperty("versionPath")]
		public string VersionPath {
			get {
				return (string)base["versionPath"];
			}
			set {
				base["versionPath"] = value;
			}
		}

		[ConfigurationProperty("updateUrl")]
		public string UpdateUrl {
			get {
				return (string)base["updateUrl"];
			}
			set {
				base["updateUrl"] = value;
			}
		}
	}
}
