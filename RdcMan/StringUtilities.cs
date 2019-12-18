using System;
using System.Collections.Generic;
using System.Globalization;

namespace RdcMan
{
	public static class StringUtilities
	{
		private const char SetOpenChar = '{';

		private const char SetSeparatorChar = ',';

		private const char SetCloseChar = '}';

		private const char RangeOpenChar = '[';

		private const char RangeSeparatorChar = '-';

		private const char RangeCloseChar = ']';

		public static string CultureFormat(this string format, params object[] args)
		{
			return string.Format(CultureInfo.CurrentUICulture, format, args);
		}

		public static string InvariantFormat(this string format, params object[] args)
		{
			return string.Format(CultureInfo.InvariantCulture, format, args);
		}

		public static IEnumerable<string> ExpandPattern(string pattern)
		{
			bool anyExpansions = false;
			for (int i = 0; i < pattern.Length; i++)
			{
				switch (pattern[i])
				{
				case '{':
				{
					int closeIndex2 = pattern.IndexOf('}', i);
					if (closeIndex2 == -1)
					{
						throw new ArgumentException($"Set not closed (missing {'}'}): {pattern.Substring(i)}");
					}
					string prefix2 = pattern.Substring(0, i);
					string suffix2 = pattern.Substring(closeIndex2 + 1);
					IEnumerable<string> setEnumerator = ExpandSet(pattern.Substring(i + 1, closeIndex2 - i - 1));
					foreach (string setValue in setEnumerator)
					{
						foreach (string suffixExpansion2 in ExpandPattern(suffix2))
						{
							yield return prefix2 + setValue + suffixExpansion2;
						}
					}
					anyExpansions = true;
					break;
				}
				case '[':
				{
					int closeIndex = pattern.IndexOf(']', i);
					if (closeIndex == -1)
					{
						throw new ArgumentException($"Range not closed (missing {']'}): {pattern.Substring(i)}");
					}
					string prefix = pattern.Substring(0, i);
					string suffix = pattern.Substring(closeIndex + 1);
					IEnumerable<string> rangeEnumerator = ExpandRange(pattern.Substring(i + 1, closeIndex - i - 1));
					foreach (string rangeValue in rangeEnumerator)
					{
						foreach (string suffixExpansion in ExpandPattern(suffix))
						{
							yield return prefix + rangeValue + suffixExpansion;
						}
					}
					anyExpansions = true;
					break;
				}
				default:
					continue;
				}
				break;
			}
			if (!anyExpansions)
			{
				yield return pattern;
			}
		}

		private static IEnumerable<string> ExpandSet(string set)
		{
			return set.Split(',');
		}

		private static IEnumerable<string> ExpandRange(string range)
		{
			string[] rangeValues = range.Split('-');
			if (rangeValues.Length != 2)
			{
				throw new ArgumentException($"Range does not contain low and high values (single {'-'} separator): {range}");
			}
			string low = rangeValues[0];
			string high = rangeValues[1];
			if (low.Length == 0 || high.Length == 0)
			{
				throw new ArgumentException($"Range is missing a value: {range}");
			}
			if (char.IsLetter(low, 0))
			{
				if (!char.IsLetter(high, 0))
				{
					throw new ArgumentException($"Range must be homogenous (letter bounds or numeric bounds): {range}");
				}
				if (low.Length != 1 || high.Length != 1)
				{
					throw new ArgumentException($"Letter range must be single character: {range}");
				}
				if (char.IsLower(low[0]) != char.IsLower(high[0]))
				{
					throw new ArgumentException($"Letter range must be same case: {range}");
				}
				if (low.CompareTo(high) > 0)
				{
					throw new ArgumentException($"Range low cannot be greater than high: {range}");
				}
				int lowValue2 = low[0];
				int highValue2 = high[0];
				for (int value2 = lowValue2; value2 <= highValue2; value2++)
				{
					yield return $"{(char)value2}";
				}
				yield break;
			}
			if (char.IsDigit(low, 0))
			{
				if (!int.TryParse(low, out int lowValue) || !int.TryParse(high, out int highValue))
				{
					throw new ArgumentException($"Range must be homogenous (letter bounds or numeric bounds): {range}");
				}
				if (lowValue > highValue)
				{
					throw new ArgumentException($"Range low cannot be greater than high: {range}");
				}
				int numDigits = low.Length;
				string format = "";
				for (int i = 0; i < numDigits; i++)
				{
					format += "0";
				}
				for (int value = lowValue; value <= highValue; value++)
				{
					yield return value.ToString(format);
				}
				yield break;
			}
			throw new ArgumentException($"Malformed range (must have letter bounds or numeric bounds): {range}");
		}
	}
}
