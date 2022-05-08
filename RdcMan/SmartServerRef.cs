namespace RdcMan {
	internal class SmartServerRef : ServerRef {
		public SmartServerRef(Server server) : base(server) { }

		public override bool ConfirmRemove(bool askUser) {
			FormTools.InformationDialog("智能组成员由纳入标准指定；不允许手动删除。");
			return false;
		}
	}
}
