namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public abstract class VersionedChecksummedBytes {
		private readonly int version;
		private readonly IReadOnlyList<byte> bytes;

		protected VersionedChecksummedBytes(string encoded) {
			byte[] tmp = Base58.DecodeChecked(encoded);
			this.version = tmp[0] & 0xFF;
			var bytes = new byte[tmp.Length - 1];
			Array.Copy(tmp, 1, bytes, 0, tmp.Length - 1);
			this.bytes = bytes;
		}

		protected VersionedChecksummedBytes(int version, byte[] bytes) {
			Requires.InRange(version < 256 && version >= 0, "version");
			this.version = version;
			this.bytes = bytes;
		}

		/// <summary>
		/// Returns the "version" or "header" byte: the first byte of the data. This is used to disambiguate what the
		/// contents apply to, for example, which network the key or address is valid on.
		/// </summary>
		/// <value>A positive number between 0 and 255.</value>
		public int Version {
			get { return this.version; }
		}

		protected IReadOnlyList<byte> Bytes {
			get { return this.bytes; }
		}

		public override int GetHashCode() {
			return Utils.GetArrayHashCode(this.bytes);
		}

		public override bool Equals(object o) {
			var other = o as VersionedChecksummedBytes;
			if (other == null) {
				return false;
			}

			return Utils.ArraysEqual(other.bytes, this.bytes);
		}

		public override string ToString() {
			// A stringified buffer is:
			//   1 byte version + data bytes + 4 bytes check code (a truncated hash)
			byte[] addressBytes = new byte[1 + bytes.Count + 4];
			addressBytes[0] = (byte)version;
			bytes.CopyTo(0, addressBytes, 1, bytes.Count);
			byte[] check = Utils.DoubleDigest(addressBytes, 0, bytes.Count + 1);
			Array.Copy(check, 0, addressBytes, bytes.Count + 1, 4);
			return Base58.Encode(addressBytes);
		}
	}
}
