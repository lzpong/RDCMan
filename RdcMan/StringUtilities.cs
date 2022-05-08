using System;
using System.Collections.Generic;
using System.Globalization;

namespace RdcMan {
	public static class StringUtilities {
		private const char SetOpenChar = '{';

		private const char SetSeparatorChar = ',';

		private const char SetCloseChar = '}';

		private const char RangeOpenChar = '[';

		private const char RangeSeparatorChar = '-';

		private const char RangeCloseChar = ']';

		public static string CultureFormat(this string format, params object[] args) {
			return string.Format(CultureInfo.CurrentUICulture, format, args);
		}

		public static string InvariantFormat(this string format, params object[] args) {
			return string.Format(CultureInfo.InvariantCulture, format, args);
		}

		public static IEnumerable<string> ExpandPattern(string pattern) {
			bool flag = false;
			for (int i = 0; i < pattern.Length; i++) {
				switch (pattern[i]) {
					case SetOpenChar: {
						int num2 = pattern.IndexOf(SetCloseChar, i);
						if (num2 == -1)
							throw new ArgumentException($"����δ�պϣ�ȱʧ {SetCloseChar}����{pattern.Substring(i)}");

						string prefix = pattern.Substring(0, i);
						string suffix = pattern.Substring(num2 + 1);
						IEnumerable<string> enumerable2 = ExpandSet(pattern.Substring(i + 1, num2 - i - 1));
						foreach (string setValue in enumerable2) {
							foreach (string item in ExpandPattern(suffix)) {
								yield return prefix + setValue + item;
							}
						}
						flag = true;
						break;
					}
					case RangeOpenChar: {
						int num = pattern.IndexOf(RangeCloseChar, i);
						if (num == -1)
							throw new ArgumentException($"��Χδ�պϣ�ȱʧ {RangeCloseChar}����{pattern.Substring(i)}");

						string suffix = pattern.Substring(0, i);
						string prefix = pattern.Substring(num + 1);
						IEnumerable<string> enumerable = ExpandRange(pattern.Substring(i + 1, num - i - 1));
						foreach (string setValue in enumerable) {
							foreach (string item2 in ExpandPattern(prefix)) {
								yield return suffix + setValue + item2;
							}
						}
						flag = true;
						break;
					}
					default:
						continue;
				}
				break;
			}
			if (!flag)
				yield return pattern;
		}

		private static IEnumerable<string> ExpandSet(string set) {
			return set.Split(SetSeparatorChar);
		}

		private static IEnumerable<string> ExpandRange(string range) {
			string[] array = range.Split(RangeSeparatorChar);
			if (array.Length != 2)
				throw new ArgumentException($"��Χ��������ֵ�͸�ֵ������ {RangeSeparatorChar} �ָ�������{range}");

			string text = array[0];
			string text2 = array[1];
			if (text.Length == 0 || text2.Length == 0)
				throw new ArgumentException($"��Χȱ��ֵ�� {range}");

			if (char.IsLetter(text, 0)) {
				if (!char.IsLetter(text2, 0))
					throw new ArgumentException($"��Χ������ͬ���͵ģ���ĸ��Χ�����ַ�Χ���� {range}");
				if (text.Length != 1 || text2.Length != 1)
					throw new ArgumentException($"��ĸ��Χ�����ǵ����ַ���{range}");
				if (char.IsLower(text[0]) != char.IsLower(text2[0]))
					throw new ArgumentException($"��ĸ��Χ������ͬ�� {range}");
				if (text.CompareTo(text2) > 0)
					throw new ArgumentException($"��Χ�Ͳ��ܴ��ڸߣ�{range}");

				int num = text[0];
				int highValue2 = text2[0];
				for (int value2 = num; value2 <= highValue2; value2++) {
					yield return $"{(char)value2}";
				}
			}
			else {
				if (!char.IsDigit(text, 0))
					throw new ArgumentException($"��ʽ����ķ�Χ����������ĸ��Χ�����ַ�Χ����{range}");
				if (!int.TryParse(text, out var result) || !int.TryParse(text2, out var highValue2))
					throw new ArgumentException($"��Χ������ͬ�ʵģ���ĸ��Χ�����ַ�Χ����{range}");
				if (result > highValue2)
					throw new ArgumentException($"��Χ�Ͳ��ܴ��ڸߣ�{range}");

				int length = text.Length;
				string format = "";
				for (int i = 0; i < length; i++) {
					format += "0";
				}
				for (int value2 = result; value2 <= highValue2; value2++) {
					yield return value2.ToString(format);
				}
			}
		}
	}
}
