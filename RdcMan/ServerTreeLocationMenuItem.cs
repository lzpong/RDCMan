using System.Windows.Forms;

namespace RdcMan
{
	internal class ServerTreeLocationMenuItem : EnumMenuItem<DockStyle>
	{
		protected override DockStyle Value
		{
			get
			{
				return Program.TheForm.ServerTreeLocation;
			}
			set
			{
				Program.TheForm.ServerTreeLocation = value;
			}
		}

		public ServerTreeLocationMenuItem(string text, DockStyle value)
			: base(text, value)
		{
		}
	}
}
