using System;
using System.Collections.Generic;
using System.Reflection;

namespace RdcMan
{
	public static class SettingExtensions
	{
		public static void GetSettingProperties(this Type type, out Dictionary<string, SettingProperty> settingProperties)
		{
			settingProperties = new Dictionary<string, SettingProperty>(StringComparer.OrdinalIgnoreCase);
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo propertyInfo in properties)
			{
				object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(SettingAttribute), inherit: false);
				if (customAttributes.Length == 1)
				{
					SettingAttribute settingAttribute = (SettingAttribute)customAttributes[0];
					settingProperties[settingAttribute.XmlName] = new SettingProperty
					{
						Property = propertyInfo,
						Attribute = settingAttribute
					};
				}
			}
		}
	}
}
