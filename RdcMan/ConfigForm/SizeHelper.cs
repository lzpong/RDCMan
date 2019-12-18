using System;
using System.Drawing;

namespace RdcMan
{
	public static class SizeHelper
	{
		public static Size[] StockSizes;

		public static readonly string Separator = " x ";

		public static readonly string AltSeparator = ", ";

		public static Size Parse(string dim)
		{
			string[] array = dim.Split(new string[2]
			{
				Separator,
				AltSeparator
			}, StringSplitOptions.None);
			if (array.Length != 2)
			{
				throw new InvalidOperationException("Bad Size string '{0}'".InvariantFormat(dim));
			}
			return FromString(array[0], array[1]);
		}

		public static Size FromString(string widthStr, string heightStr)
		{
			int width = int.Parse(widthStr);
			int height = int.Parse(heightStr);
			return new Size(width, height);
		}

		public static string ToFormattedString(this Size size)
		{
			return "{0}{1}{2}".InvariantFormat(size.Width, Separator, size.Height);
		}
	}
}
