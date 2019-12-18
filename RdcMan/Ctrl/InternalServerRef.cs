using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RdcMan.Ctrl {
	class InternalServerRef : ServerRef {
		public InternalServerRef(Server server) : base(server) { }

		//public override bool CanRemove(bool popUI) {
		//	return false;//AllowEdit(popUI);
		//}

	}
}
