using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Win32;

namespace RdcMan
{
	public static class Helpers
	{
		public delegate void ReadXmlDelegate(XmlNode childNode, RdcTreeNode node, ICollection<string> errors);

		private class OperationTimer : IDisposable
		{
			private Stopwatch _stopWatch;

			private string _text;

			public OperationTimer(string text)
			{
				_text = text;
				_stopWatch = new Stopwatch();
				_stopWatch.Start();
				Log.Write("Started {0}", text);
				Log.AdjustIndent(1);
			}

			void IDisposable.Dispose()
			{
				_stopWatch.Stop();
				Log.AdjustIndent(-1);
				Log.Write("Finished {0}: {1} ms", _text, _stopWatch.ElapsedMilliseconds);
			}
		}

		public static bool IsControlKeyPressed
		{
			get
			{
				short asyncKeyState = User.GetAsyncKeyState(17);
				return (asyncKeyState & 0x8000) != 0;
			}
		}

		public static void Add(this Control.ControlCollection collection, params Control[] controls)
		{
			collection.AddRange(controls);
		}

		public static string GetName(this XmlNode node)
		{
			XmlAttribute xmlAttribute = node.Attributes["name"];
			return xmlAttribute != null ? xmlAttribute.Value : node.Name;
		}

		public static string GetFullPath(this XmlNode node)
		{
			StringBuilder stringBuilder = new StringBuilder(node.GetName());
			XmlNode parentNode = node.ParentNode;
			while (parentNode != null && !(parentNode is XmlDocument))
			{
				stringBuilder.Insert(0, "/");
				stringBuilder.Insert(0, parentNode.GetName());
				parentNode = parentNode.ParentNode;
			}
			return stringBuilder.ToString();
		}

		public static void ForEach<TObject>(this IEnumerable<TObject> objects, Action<TObject> action)
		{
			foreach (TObject @object in objects)
			{
				action(@object);
			}
		}

		public static void ForEach(this TreeNodeCollection objects, Action<TreeNode> action)
		{
			foreach (TreeNode @object in objects)
			{
				action(@object);
			}
		}

		public static TEnum ParseEnum<TEnum>(this string value) where TEnum : struct
		{
			return (TEnum)Enum.Parse(typeof(TEnum), value);
		}

		public static IEnumerable<TEnum> EnumValues<TEnum>() where TEnum : struct
		{
			return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
		}

		public static string SortOrderToString(SortOrder sortOrder) {
			return sortOrder switch {
				SortOrder.ByName => "Ãû³Æ",
				SortOrder.ByStatus => "×´Ì¬.Ãû³Æ",
				SortOrder.None => "²»ÅÅÐò",
				_ => throw new Exception("Unexpected SortOrder"),
			};
		}

		public static string GetTemporaryFileName(string fileName, string suffix)
		{
			string text = fileName + suffix;
			int num = 0;
			while (File.Exists(text))
			{
				text = fileName + suffix + num++;
			}
			return text;
		}

		public static void MoveTemporaryToPermanent(string newFileName, string fileName, bool saveOld)
		{
			string temporaryFileName = GetTemporaryFileName(fileName, ".old");
			if (File.Exists(fileName))
			{
				File.Move(fileName, temporaryFileName);
			}
			File.Move(newFileName, fileName);
			if (!saveOld)
			{
				File.Delete(temporaryFileName);
			}
		}

		public static int NaturalCompare(string x, string y)
		{
			int i = 0;
			int i2 = 0;
			while (i < x.Length && i2 < y.Length)
			{
				char c = char.ToLowerInvariant(x[i]);
				char c2 = char.ToLowerInvariant(y[i]);
				if (char.IsDigit(c) && char.IsDigit(c2))
				{
					uint num = ParseNumber(x, ref i);
					uint num2 = ParseNumber(y, ref i2);
					if (num != num2)
						return num >= num2 ? 1 : -1;

					if (i != i2)
					{
						return i2 - i;
					}
				}
				else
				{
					if (c != c2)
					{
						return c - c2;
					}
					i++;
					i2++;
				}
			}
			return x.Length - y.Length;
		}

		private static uint ParseNumber(string s, ref int i)
		{
			uint num = (uint)(s[i] - 48);
			while (++i < s.Length && char.IsDigit(s[i]))
			{
				num = num * 10 + s[i] - 48;
			}
			return num;
		}

		public static IDisposable Timer(string format, params object[] args)
		{
			return new OperationTimer(format.InvariantFormat(args));
		}

		public static string FormatTime(int seconds) {
			int h = seconds / 3600;
			seconds %= 3600;
			int m = seconds / 60;
			seconds %= 60;
			return $"{h:00}:{m:00}:{seconds:00}";
		}

		//private static void AppendUnitValue(StringBuilder builder, int value, string unit) {
		//	builder.AppendFormat("{0} {1}", value, unit);
		//}
	}
}
