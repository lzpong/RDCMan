namespace RdcMan {
	internal abstract class VirtualGroup : GroupBase {
		protected IServerRefFactory ServerRefFactory => this as IServerRefFactory;

		protected VirtualGroup() {
			ChangeImageIndex(ImageConstants.Group);
		}

		protected override void InitSettings() {
			base.Properties = new GroupSettings();
			base.InitSettings();
		}

		public sealed override bool CanAddServers() {
			return false;
		}

		public sealed override bool CanAddGroups() {
			return false;
		}

		public sealed override bool CanDropGroups() {
			return false;
		}
	}
}
