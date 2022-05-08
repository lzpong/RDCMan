using System;
using System.Configuration;
using System.Drawing;

namespace RdcMan.Configuration {
	public static class Current {
		public static RdcManSection RdcManSection { get; private set; }

		public static void Read() {
			System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			RdcManSection = configuration.GetSection("rdcman") as RdcManSection;
			if (RdcManSection != null) {
				DisplaySizeElementCollection displaySizes = RdcManSection.DisplaySizes;
				int num = Math.Min(10, displaySizes.Count);
				SizeHelper.StockSizes = new Size[num];
				for (int i = 0; i < num; i++) {
					SizeHelper.StockSizes[i] = SizeHelper.Parse(displaySizes.GetDisplaySize(i).Size);
				}
			}
			else {
				RdcManSection = new RdcManSection();
				SizeHelper.StockSizes = new Size[7];
				SizeHelper.StockSizes[0] = new Size(800, 600);
				SizeHelper.StockSizes[1] = new Size(1024, 768);
				SizeHelper.StockSizes[2] = new Size(1280, 1024);
				SizeHelper.StockSizes[3] = new Size(1366, 768);
				SizeHelper.StockSizes[4] = new Size(1440, 900);
				SizeHelper.StockSizes[5] = new Size(1600, 1200);
				SizeHelper.StockSizes[6] = new Size(1920, 1200);
			}
		}
	}
}
