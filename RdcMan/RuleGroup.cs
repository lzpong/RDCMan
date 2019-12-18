using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace RdcMan
{
	internal class RuleGroup
	{
		public const string XmlNodeName = "ruleGroup";

		private const string GroupingOperatorXmlNodeName = "operator";

		private static Dictionary<string, Helpers.ReadXmlDelegate> NodeActions = new Dictionary<string, Helpers.ReadXmlDelegate>();

		public RuleGroupOperator Operator
		{
			get;
			private set;
		}

		public List<Rule> Rules
		{
			get;
			private set;
		}

		public RuleGroup(RuleGroupOperator op, IEnumerable<Rule> rules)
		{
			Set(op, rules);
		}

		protected RuleGroup()
		{
		}

		public static RuleGroup Create(XmlNode xmlNode, RdcTreeNode node, ICollection<string> errors)
		{
			RuleGroup ruleGroup = new RuleGroup();
			ruleGroup.ReadXml(xmlNode, node, errors);
			return ruleGroup;
		}

		public void Set(RuleGroupOperator op, IEnumerable<Rule> rules)
		{
			Operator = op;
			Rules = rules.ToList();
		}

		public bool Evaluate(Server server)
		{
			bool result = false;
			bool result2 = true;
			foreach (Rule rule in Rules)
			{
				if (rule.Evaluate(server))
				{
					result = true;
				}
				else
				{
					result2 = false;
				}
			}
			if (Operator != 0)
			{
				return result2;
			}
			return result;
		}

		public void ReadXml(XmlNode xmlNode, RdcTreeNode node, ICollection<string> errors)
		{
			Operator = xmlNode.Attributes["operator"].Value.ParseEnum<RuleGroupOperator>();
			Rules = new List<Rule>();
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				Rules.Add(Rule.Create(childNode, node, errors));
			}
		}

		public void WriteXml(XmlTextWriter tw)
		{
			tw.WriteStartElement("ruleGroup");
			tw.WriteAttributeString("operator", Operator.ToString());
			Rules.ForEach(delegate(Rule r)
			{
				r.WriteXml(tw);
			});
			tw.WriteEndElement();
		}
	}
}
