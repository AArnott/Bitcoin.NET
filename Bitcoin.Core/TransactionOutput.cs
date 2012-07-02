using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bitcoin.Core {
	class TransactionOutput {
		private NetworkParameters n;
		private Transaction t;
		private byte[] p;

		public TransactionOutput(NetworkParameters n, Transaction t, byte[] p) {
			// TODO: Complete member initialization
			this.n = n;
			this.t = t;
			this.p = p;
		}
	}
}
