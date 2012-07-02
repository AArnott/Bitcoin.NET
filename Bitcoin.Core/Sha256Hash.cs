namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Numerics;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;

	[Serializable]
	public class Sha256Hash {
		private const long serialVersionUID = 3778897922647016546L;

		private byte[] bytes;
		private int hash = -1;

		/**
		 * @see setHashcodeByteLength(int hashcodeByteLength)
		 */
		private const int HASHCODE_BYTES_TO_CHECK = 5;


		public static readonly Sha256Hash ZERO_HASH = new Sha256Hash(new byte[32]);

		/**
		 * Creates a Sha256Hash by wrapping the given byte array. It must be 32 bytes long.
		 */
		public Sha256Hash(byte[] bytes) {
			Requires.True(bytes.Length == 32);
			this.bytes = bytes;

		}

		private Sha256Hash(byte[] bytes, int hash) {
			Requires.True(bytes.Length == 32);
			this.bytes = bytes;
			this.hash = hash;
		}

		/**
		 * Creates a Sha256Hash by decoding the given hex string. It must be 64 characters long.
		 */
		public Sha256Hash(String value) {
			Requires.True(value.Length == 64);
			this.bytes = Hex.Decode(value);
		}

		/**
		 * Calculates the (one-time) hash of contents and returns it as a new wrapped hash.
		 */
		public static Sha256Hash create(byte[] contents) {
			using (var hashAlgorithm = HashAlgorithm.Create("SHA-256")) {
				return new Sha256Hash(hashAlgorithm.ComputeHash(contents));
			}
		}

		/**
		 * Returns true if the hashes are equal.
		 */
		public override bool Equals(object other) {
			if (!(other is Sha256Hash)) return false;
			return Utils.ArraysEqual(bytes, ((Sha256Hash)other).bytes);
		}

		/**
		 * Hash code of the byte array as calculated by {@link Arrays#hashCode()}. Note the difference between a SHA256
		 * secure bytes and the type of quick/dirty bytes used by the Java hashCode method which is designed for use in
		 * bytes tables.
		 */
		public override int GetHashCode() {
			if (hash == -1) {
				hash = 1;
				for (int i = 0; i < HASHCODE_BYTES_TO_CHECK; i++)
					hash = 31 * hash + bytes[i];
			}
			return hash;
		}

		public override String ToString() {
			return Hex.Encode(this.bytes);
		}

		/**
		 * Returns the bytes interpreted as a positive integer.
		 */
		public BigInteger toBigInteger() {
			return new BigInteger(bytes); // TODO: is this a positive integer?
		}

		public byte[] getBytes() {
			return bytes;
		}

		public Sha256Hash duplicate() {
			return new Sha256Hash(bytes, hash);
		}
	}
}
