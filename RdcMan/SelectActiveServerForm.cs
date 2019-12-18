using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RdcMan
{
	internal class SelectActiveServerForm : RdcDialog
	{
		public enum Operation
		{
			SelectServer,
			SelectTree,
			MinimizeWindow
		}

		public class SelectedObject
		{
			public Operation Operation;

			public char Key;

			public ServerBase Server;
		}

		public SelectedObject Selected
		{
			get;
			private set;
		}

		public SelectActiveServerForm(IEnumerable<ServerBase> servers)
			: base("Select server", null)
		{
			BackColor = Color.White;
			base.ClientSize = new Size(304, FormTools.YPos(7));
			int num = 0;
			foreach (ServerBase item in servers.Take(10))
			{
				char key = (num == 9) ? '0' : ((char)(49 + num));
				AddButton(num / 5, num % 5, key, item.DisplayName, Operation.SelectServer, item);
				num++;
			}
			num += num % 2;
			int rowIndex = (num < 10) ? (num % 5) : 5;
			AddButton(0, rowIndex, 'T', "Server Tree", Operation.SelectTree, null);
			AddButton(1, rowIndex, 'N', "Minimize RDCMan", Operation.MinimizeWindow, null);
			base.KeyDown += List_KeyDownHandler;
			this.ScaleAndLayout();
		}

		private void AddButton(int colIndex, int rowIndex, char key, string text, Operation operation, ServerBase server)
		{
			Button button = new Button();
			button.Location = FormTools.NewLocation(colIndex, rowIndex);
			button.FlatStyle = FlatStyle.Flat;
			button.Text = $"{key} - {text}";
			button.Tag = new SelectedObject
			{
				Key = key,
				Operation = operation,
				Server = server
			};
			Button button2 = button;
			button2.Width = 140;
			button2.Click += Button_Click;
			button2.KeyDown += Button_KeyDown;
			base.Controls.Add(button2);
		}

		private void Button_KeyDown(object sender, KeyEventArgs e)
		{
			List_KeyDownHandler(sender, e);
			if (base.DialogResult == DialogResult.None)
			{
				e.Handled = false;
			}
		}

		private void Button_Click(object sender, EventArgs e)
		{
			SelectedObject o = (sender as Button).Tag as SelectedObject;
			SelectObject(o);
		}

		private void List_KeyDownHandler(object sender, KeyEventArgs e)
		{
			char c = (char)e.KeyData;
			if (e.KeyData >= Keys.NumPad0 && e.KeyData <= Keys.NumPad9)
			{
				c = (char)(e.KeyData - 96 + 48);
			}
			if (c >= 'a' && c <= 'z')
			{
				c = (char)(c - 97 + 65);
			}
			foreach (Control control in base.Controls)
			{
				SelectedObject selectedObject = control.Tag as SelectedObject;
				if (selectedObject != null && selectedObject.Key == c)
				{
					SelectObject(selectedObject);
					break;
				}
			}
			if (e.KeyData == Keys.Escape)
			{
				Cancel();
			}
			e.Handled = true;
		}

		private void SelectObject(SelectedObject o)
		{
			Selected = o;
			OK();
		}
	}
}
