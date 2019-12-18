using System;
using System.Collections.Generic;

namespace RdcMan
{
	public class ArgumentParser
	{
		public Dictionary<string, bool> Switches = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

		public Dictionary<string, string> SwitchValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public List<string> PlainArgs = new List<string>();

		public void AddSwitch(string name, bool requiresValue)
		{
			Switches[name] = requiresValue;
		}

		public void Parse()
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			int num = 1;
			while (true)
			{
				if (num >= commandLineArgs.Length)
				{
					return;
				}
				if (IsSwitch(commandLineArgs[num]))
				{
					string key = commandLineArgs[num].Substring(1);
					if (!Switches.TryGetValue(key, out bool value))
					{
						throw new ArgumentException("Unexpected switch: " + commandLineArgs[num]);
					}
					string value2 = string.Empty;
					if (value)
					{
						if (num >= commandLineArgs.Length - 1)
						{
							break;
						}
						value2 = commandLineArgs[++num];
					}
					SwitchValues[key] = value2;
				}
				else
				{
					PlainArgs.Add(commandLineArgs[num]);
				}
				num++;
			}
			throw new ArgumentException("Switch " + commandLineArgs[num] + " requires an argument");
		}

		public bool HasSwitch(string name)
		{
			string value;
			return SwitchValues.TryGetValue(name, out value);
		}

		private bool IsSwitch(string arg)
		{
			char c = arg[0];
			if (c != '/')
			{
				return c == '-';
			}
			return true;
		}
	}
}
