using System;
using System.Configuration;
using System.Drawing;
using System.IO;

namespace RdcMan.Configuration
{
	public static class Current
	{
		public static RdcManSection RdcManSection
		{
			get;
			private set;
		}

        static void CheckConfig() {
            if (!File.Exists("RDCMan.exe.config")) {
                using (FileStream fs = new FileStream("RDCMan.exe.config", FileMode.OpenOrCreate)) {
                    byte[] bs = System.Text.Encoding.ASCII.GetBytes("<?xml version=\"1.0\"?>\r\n" +
"<configuration>\r\n" +
"  <configSections>\r\n    <section name=\"rdcman\" type=\"RdcMan.Configuration.RdcManSection, RDCMan\"/>\r\n  </configSections>\r\n" +
"\r\n" +
"  <startup>\r\n    <supportedRuntime version=\"v4.0\" sku=\".NETFramework,Version=v4.0\"/>\r\n  </startup>\r\n" +
"\r\n" +
"  <rdcman>\r\n" +
"    <programUpdate>\r\n        <versionPath></versionPath>\r\n        <updateUrl></updateUrl>\r\n    </programUpdate>\r\n" +
"\r\n" +
"    <warningThresholds>\r\n        <connect></connect>\r\n    </warningThresholds>\r\n"+
"\r\n" +
"    <logging>\r\n        <enabled></enabled>\r\n        <path></path>\r\n        <maximumNumberOfFiles></maximumNumberOfFiles>\r\n    </logging>\r\n" +
"\r\n" +
"    <!-- Size options for client size and remote desktop size. Only the first ten are used. -->\r\n" +
"    <displaySizes>\r\n      <add size=\"800 x 600\"/>\r\n      <add size=\"1024 x 768\"/>\r\n      <add size=\"1280 x 1024\"/>\r\n     <add size=\"1366 x 768\"/>\r\n      <add size=\"1440 x 900\"/>\r\n      <add size=\"1600 x 1200\"/>\r\n      <add size=\"1920 x 1200\"/>\r\n    </displaySizes>\r\n" +
"  </rdcman>\r\n" +
"</configuration>");
                    fs.Write(bs, 0, bs.Length);
                }
            }
        }

		public static void Read()
		{
            CheckConfig();
			System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			RdcManSection = (configuration.GetSection("rdcman") as RdcManSection);
            if (RdcManSection == null) {
                FormTools.InformationDialog("RDCMan Œ¥’“µΩ ≈‰÷√Œƒº˛: RDCMan.exe.config");
                return;
            }
			DisplaySizeElementCollection displaySizes = RdcManSection.DisplaySizes;
			int num = Math.Min(10, displaySizes.Count);
			SizeHelper.StockSizes = new Size[num];
			for (int i = 0; i < num; i++)
			{
				SizeHelper.StockSizes[i] = SizeHelper.Parse(displaySizes.GetDisplaySize(i).Size);
			}
		}
	}
}
