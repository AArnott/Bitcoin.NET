namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;

	internal static class Utils {
		/// <summary>
		/// Calculates the SHA-256 hash of the given byte range, and then hashes the resulting hash again. This is
		/// standard procedure in BitCoin. The resulting hash is in big endian form.
		/// </summary>
		internal static byte[] DoubleDigest(byte[] input) {
			Requires.NotNull(input, "input");
			return DoubleDigest(input, 0, input.Length);
		}

		/// <summary>
		/// Calculates the SHA-256 hash of the given byte range, and then hashes the resulting hash again. This is
		/// standard procedure in BitCoin. The resulting hash is in big endian form.
		/// </summary>
		internal static byte[] DoubleDigest(byte[] input, int offset, int count) {
			Requires.NotNull(input, "input");
			Requires.InRange(offset < input.Length, "offset");
			Requires.InRange(offset + count <= input.Length, "count");

			using (var hashAlgorithm = HashAlgorithm.Create("SHA-256")) {
				byte[] hash = hashAlgorithm.ComputeHash(input, offset, count);
				return hashAlgorithm.ComputeHash(hash);
			}
		}

		internal static bool ArraysEqual(byte[] array1, byte[] array2) {
			Requires.NotNull(array1, "array1");
			Requires.NotNull(array2, "array2");

			if (array1.Length != array2.Length) {
				return false;
			}

			for (int i = 0; i < array1.Length; i++) {
				if (array1[i] != array2[i]) {
					return false;
				}
			}

			return true;
		}
	}
}
