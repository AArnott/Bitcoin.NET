namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class Block {
		private NetworkParameters n;
		public static long EASIEST_DIFFICULTY_TARGET;

		public Block(NetworkParameters n) {
			// TODO: Complete member initialization
			this.n = n;
		}

		internal void addTransaction(Transaction t) {
			throw new NotImplementedException();
		}

		internal void setTime(long p) {
			throw new NotImplementedException();
		}

		internal void setDifficultyTarget(long p) {
			throw new NotImplementedException();
		}

		internal void setNonce(int p) {
			throw new NotImplementedException();
		}

		internal string getHashAsString() {
			throw new NotImplementedException();
		}
	}
}
