using System;
using System.Collections.Generic;

namespace RdcMan {
	public class ArgumentParser {
		public Dictionary<string, bool> Switches = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

		public Dictionary<string, string> SwitchValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public List<string> PlainArgs = new List<string>();

		public void AddSwitch(string name, bool requiresValue) {
			Switches[name] = requiresValue;
		}

		public void Parse() {
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 1; i < commandLineArgs.Length; i++) {
				if (IsSwitch(commandLineArgs[i])) {
					string key = commandLineArgs[i].Substring(1);
					if (!Switches.TryGetValue(key, out var value)) {
						throw new ArgumentException("Unexpected switch: " + commandLineArgs[i]);
					}
					string value2 = string.Empty;
					if (value) {
						if (i >= commandLineArgs.Length - 1) {
							throw new ArgumentException("Switch " + commandLineArgs[i] + " requires an argument");
						}
						value2 = commandLineArgs[++i];
					}
					SwitchValues[key] = value2;
				}
				else {
					PlainArgs.Add(commandLineArgs[i]);
				}
			}
		}

		public bool HasSwitch(string name) {
			string value;
			return SwitchValues.TryGetValue(name, out value);
		}

		private bool IsSwitch(string arg) {
			char c = arg[0];
			if (c != '/') {
				return c == '-';
			}
			return true;
		}
	}
}
