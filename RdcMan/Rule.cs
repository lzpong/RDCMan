using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace RdcMan
{
	public class Rule
	{
		public const string XmlNodeName = "rule";

		private const string PropertyXmlNodeName = "property";

		private const string OperatorXmlNodeName = "operator";

		private const string ValueXmlNodeName = "value";

		public RuleProperty Property
		{
			get;
			private set;
		}

		public RuleOperator Operator
		{
			get;
			private set;
		}

		public object Value
		{
			get;
			private set;
		}

		public Rule(RuleProperty property, RuleOperator operation, object value)
		{
			Property = property;
			Operator = operation;
			Value = value;
		}

		protected Rule()
		{
		}

		public static Rule Create(XmlNode xmlNode, RdcTreeNode node, ICollection<string> errors)
		{
			Rule rule = new Rule();
			rule.ReadXml(xmlNode, node, errors);
			return rule;
		}

		public bool Evaluate(Server server)
		{
			bool isString;
			object obj = Property.GetValue(server, out isString);
			if (obj == null)
			{
				obj = string.Empty;
			}
			return Regex.IsMatch((string)obj, (string)Value, RegexOptions.IgnoreCase) ^ (Operator == RuleOperator.DoesNotMatch);
		}

		public void ReadXml(XmlNode xmlNode, RdcTreeNode node, ICollection<string> errors)
		{
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				switch (childNode.Name)
				{
				case "property":
					Property = new RuleProperty(childNode.InnerText.ParseEnum<ServerProperty>());
					break;
				case "operator":
					Operator = childNode.InnerText.ParseEnum<RuleOperator>();
					break;
				case "value":
					Value = childNode.InnerText;
					break;
				default:
					throw new NotImplementedException();
				}
			}
		}

		public void WriteXml(XmlTextWriter tw)
		{
			tw.WriteStartElement("rule");
			tw.WriteElementString("property", Property.ServerProperty.ToString());
			tw.WriteElementString("operator", Operator.ToString());
			tw.WriteElementString("value", Value.ToString());
			tw.WriteEndElement();
		}

		public override string ToString()
		{
			return "{0} {1} {2}".InvariantFormat(Property.ServerProperty, Operator, Value);
		}
	}
}
