namespace RdcMan
{
	internal class SmartServerRef : ServerRef
	{
		public SmartServerRef(Server server)
			: base(server)
		{
		}

		public override bool ConfirmRemove(bool askUser)
		{
			FormTools.InformationDialog("Smart group members are specified by inclusion criteria; manual removal is not allowed");
			return false;
		}
	}
}
