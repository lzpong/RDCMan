namespace RdcMan {
	internal class SmartServerRef : ServerRef {
		public SmartServerRef(Server server) : base(server) { }

		public override bool ConfirmRemove(bool askUser) {
			FormTools.InformationDialog("�������Ա�������׼ָ�����������ֶ�ɾ����");
			return false;
		}
	}
}
