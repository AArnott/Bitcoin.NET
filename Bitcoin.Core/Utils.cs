namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;

	public static class Utils {
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

		internal static bool ArraysEqual(IReadOnlyList<byte> array1, IReadOnlyList<byte> array2) {
			Requires.NotNull(array1, "array1");
			Requires.NotNull(array2, "array2");

			if (array1.Count != array2.Count) {
				return false;
			}

			for (int i = 0; i < array1.Count; i++) {
				if (array1[i] != array2[i]) {
					return false;
				}
			}

			return true;
		}

		internal static int GetArrayHashCode(IReadOnlyList<byte> array) {
			Requires.NotNull(array, "array");

			// TODO: code here
			return 0;
		}

		internal static void CopyTo(this IReadOnlyList<byte> sourceBuffer, int sourceIndex, byte[] destinationBuffer, int destinationIndex, int length) {
			Requires.NotNull(sourceBuffer, "sourceBuffer");
			Requires.InRange(sourceIndex >= 0 && sourceIndex < sourceBuffer.Count, "sourceIndex");
			Requires.NotNull(destinationBuffer, "destinationBuffer");
			Requires.InRange(destinationIndex >= 0 && destinationIndex < destinationBuffer.Length, "destinationIndex");
			Requires.InRange(destinationIndex + length < destinationBuffer.Length, "length");
			Requires.InRange(sourceIndex + length < sourceBuffer.Count, "length");

			while (length-- > 0) {
				destinationBuffer[destinationIndex++] = sourceBuffer[sourceIndex++];
			}
		}

		internal static byte[] MutableCopy(this IReadOnlyList<byte> readOnlyBuffer) {
			Requires.NotNull(readOnlyBuffer, "readOnlyBuffer");

			var buffer = new byte[readOnlyBuffer.Count];
			for (int i = 0; i < readOnlyBuffer.Count; i++) {
				buffer[i] = readOnlyBuffer[i];
			}

			return buffer;
		}

		public static byte[] ReverseOrder(this IReadOnlyList<byte> input) {
			Requires.NotNull(input, "input");

			var buffer = new byte[input.Count];
			for (int i = 0; i < input.Count; i++) {
				buffer[input.Count - i - 1] = input[i];
			}

			return buffer;
		}

		internal static System.Numerics.BigInteger decodeCompactBits(long p) {
			throw new NotImplementedException();
		}
	}
}
