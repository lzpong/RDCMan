using System.Windows.Forms;

namespace RdcMan {
	public interface IUndockedServerForm {
		MenuStrip MainMenuStrip { get; }

		ServerBase Server { get; }
	}
}
