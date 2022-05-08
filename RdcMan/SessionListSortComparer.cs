using System.Collections;
using System.Windows.Forms;

namespace RdcMan {
	public class SessionListSortComparer : IComparer {
		private readonly int[] _sortOrder;

		public SessionListSortComparer(int[] sortOrder) {
			_sortOrder = sortOrder;
		}

		public int Compare(object obj1, object obj2) {
			ListViewItem listViewItem = obj1 as ListViewItem;
			ListViewItem listViewItem2 = obj2 as ListViewItem;
			int[] sortOrder = _sortOrder;
			foreach (int index in sortOrder) {
				int num = string.Compare(listViewItem.SubItems[index].Text, listViewItem2.SubItems[index].Text);
				if (num != 0)
					return num;
			}
			return string.Compare(listViewItem.SubItems[0].Text, listViewItem2.SubItems[0].Text);
		}
	}
}
