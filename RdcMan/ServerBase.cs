using System.Drawing;
using System.Windows.Forms;

namespace RdcMan
{
	public abstract class ServerBase : RdcTreeNode
	{
		public enum DisplayStates
		{
			Invalid,
			Normal,
			Thumbnail
		}

		public string ServerName => ServerNode.Properties.ServerName.Value;

		public string DisplayName => ServerNode.Properties.DisplayName.Value;

		public abstract string RemoveTypeDescription
		{
			get;
		}

		public abstract bool IsConnected
		{
			get;
		}

		public abstract bool IsClientDocked
		{
			get;
		}

		public abstract bool IsClientUndocked
		{
			get;
		}

		public abstract bool IsClientFullScreen
		{
			get;
		}

		public new ServerSettings Properties
		{
            get {
                return base.Properties as ServerSettings;
            }
            set {
                base.Properties = value;
            }
        }

		public new CommonDisplaySettings DisplaySettings
		{
            get {
                return base.DisplaySettings as CommonDisplaySettings;
            }
            set {
                base.DisplaySettings = value;
            }
        }

		public bool IsThumbnail => DisplayState == DisplayStates.Thumbnail;

		public abstract DisplayStates DisplayState
		{
			get;
			set;
		}

		public abstract Size Size
		{
			get;
			set;
		}

		public abstract Point Location
		{
			get;
			set;
		}

		public abstract Server ServerNode
		{
			get;
		}

		public override bool ConfirmRemove(bool askUser)
		{
			if (!CanRemove(popUI: true))
			{
				return false;
			}
			if (askUser)
			{
				string text = "Remove '{0}' {1} from '{2}'?".InvariantFormat(base.Text, RemoveTypeDescription, base.Parent.Text);
				DialogResult dialogResult = FormTools.YesNoDialog(ParentForm, text, MessageBoxDefaultButton.Button1);
				if (dialogResult != DialogResult.Yes)
				{
					return false;
				}
			}
			return true;
		}

		internal abstract void Focus();

		internal abstract void FocusConnectedClient();

		internal abstract void ScreenCapture();

		internal abstract void GoFullScreen();

		internal abstract void LeaveFullScreen();

		internal abstract void Undock();

		internal abstract void Dock();
	}
}
