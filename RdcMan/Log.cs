using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using RdcMan.Configuration;

namespace RdcMan
{
	public static class Log
	{
		private static int Indent;

		private static TextWriter Writer;

		public static bool Enabled { get; private set; }

		public static void Init()
		{
			LoggingElement loggingElement = Current.RdcManSection?.Logging;
			Enabled = loggingElement?.Enabled ?? false;
			if (Enabled)
			{
				string text = Environment.ExpandEnvironmentVariables(loggingElement.Path);
				string format = "RDCMan-{0}.log";
				foreach (FileInfo item in (from n in Directory.GetFiles(text, format.InvariantFormat("*"), SearchOption.TopDirectoryOnly)
					select new FileInfo(n) into i
					orderby i.CreationTime descending
					select i).Skip(loggingElement.MaximumNumberOfFiles - 1))
				{
					try
					{
						item.Delete();
					}
					catch
					{
					}
				}
				string text2 = DateTime.Now.ToString("yyyyMMddHHmm");
				string path = Path.Combine(text, format.InvariantFormat(text2));
				Stream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
				Writer = new StreamWriter(stream);
			}
			Write("RDCMan v{0} build {1}", Program.TheForm.VersionText, Program.TheForm.BuildText);
			Write(Environment.OSVersion.ToString());
			Write(".NET v{0}".InvariantFormat(Environment.Version));
			Write("mstscax.dll v{0}".InvariantFormat(RdpClient.RdpControlVersion));
		}

		public static void Write(string format, params object[] args)
		{
			if (Enabled)
			{
				string value = "{0} {1} {2}".InvariantFormat(DateTime.Now.ToString("s"), new string(' ', Indent * 2), format.InvariantFormat(args));
				Writer.WriteLine(value);
				Writer.Flush();
			}
		}

		public static void AdjustIndent(int delta)
		{
			Indent += delta;
		}

		public static void DumpObject<T>(T o)
		{
			HashSet<object> visited = new HashSet<object>();
			Type typeFromHandle = typeof(T);
			Write("Fields of {0}:", typeFromHandle);
			DumpObject(o, visited);
		}

		private static void DumpObject<T>(T o, HashSet<object> visited)
		{
			Type typeFromHandle = typeof(T);
			DumpObject(o, typeFromHandle, visited);
		}

		private static void DumpObject<T>(T o, Type type, HashSet<object> visited)
		{
			AdjustIndent(1);
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.GetGetMethod() != null && propertyInfo.Name.IndexOf("password", StringComparison.OrdinalIgnoreCase) == -1)
				{
					try
					{
						object value = propertyInfo.GetValue(o, null);
						DumpValue(propertyInfo, value, visited);
					}
					catch (Exception ex)
					{
						Write("{0} exception when processing: {1}", propertyInfo.Name, ex.Message);
					}
				}
			}
			AdjustIndent(-1);
		}

		private static void DumpValue(PropertyInfo prop, object value, HashSet<object> visited)
		{
			Type propertyType = prop.PropertyType;
			if (value == null || propertyType.IsPrimitive || propertyType.IsEnum)
			{
				Write("{0} {1} = {2}", propertyType.Name, prop.Name, value ?? "{null}");
			}
			else if (propertyType.FullName.Equals("System.String"))
			{
				Write("{0} {1} = '{2}'", propertyType.Name, prop.Name, value);
			}
			else if (propertyType.IsArray)
			{
				Write("{0} {1}", propertyType.Name, prop.Name);
			}
			else if (visited.Add(value))
			{
				Write("{0} {1}", propertyType.Name, prop.Name);
				DumpObject(value, propertyType, visited);
			}
			else
			{
				Write("{0} is a recursive reference", prop.Name);
			}
		}
	}
}
