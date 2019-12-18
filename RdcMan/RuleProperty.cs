using System;

namespace RdcMan
{
	public class RuleProperty
	{
		public ServerProperty ServerProperty
		{
			get;
			private set;
		}

		public RuleProperty(ServerProperty property)
		{
			ServerProperty = property;
		}

		public object GetValue(Server server, out bool isString)
		{
			switch (ServerProperty)
			{
			case ServerProperty.DisplayName:
				isString = true;
				return server.DisplayName;
			case ServerProperty.ServerName:
				isString = true;
				return server.ServerName;
			case ServerProperty.Comment:
				isString = true;
				return server.Properties.Comment.Value;
			case ServerProperty.Parent:
				isString = true;
				return server.ParentPath;
			default:
				throw new NotImplementedException();
			}
		}
	}
}
