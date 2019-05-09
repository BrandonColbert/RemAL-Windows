using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemAL {
	interface ConnectionManager {
		void Enable();
		void Disable();
		string GetName();
	}
}
