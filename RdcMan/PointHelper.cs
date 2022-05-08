using System;
using System.Drawing;

namespace RdcMan {
	public static class PointHelper {
		public static readonly string Separator = ", ";

		public static Point Parse(string s) {
			string[] array = s.Split(new string[1] { Separator }, StringSplitOptions.None);
			if (array.Length != 2)
				throw new InvalidOperationException("Bad Point string ¡°{0}¡±".InvariantFormat(s));

			return new Point(int.Parse(array[0]), int.Parse(array[1]));
		}

		public static string ToFormattedString(this Point point) {
			return "{0}{1}{2}".InvariantFormat(point.X, Separator, point.Y);
		}
	}
}
