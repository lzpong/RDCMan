using System.Windows.Forms;

namespace RdcMan
{
	public interface ISettingsTabPage
	{
		InheritanceControl InheritanceControl { get; }

		Control FocusControl { get; }

		void UpdateControls();

		bool Validate();

		void UpdateSettings();
	}
}
