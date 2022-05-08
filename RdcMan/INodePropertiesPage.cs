using System;

namespace RdcMan {
	public interface INodePropertiesPage {
		GroupBase ParentGroup { get; }

		event Action<GroupBase> ParentGroupChanged;

		bool PopulateParentDropDown(GroupBase excludeGroup, GroupBase defaultParent);

		void SetParentDropDown(GroupBase group);
	}
}
