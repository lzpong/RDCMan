namespace RdcMan
{
	internal class FavoriteServerRef : ServerRef
	{
		public FavoriteServerRef(Server server) : base(server){}

		public override bool CanRemove(bool popUI)
		{
			return AllowEdit(popUI);
		}
	}
}
