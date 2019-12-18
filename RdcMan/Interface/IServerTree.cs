namespace RdcMan
{
	public interface IServerTree
	{
		GroupBase RootNode
		{
			get;
		}

		void AddNode(RdcTreeNode node, GroupBase parent);

		void RemoveNode(RdcTreeNode node);
	}
}
