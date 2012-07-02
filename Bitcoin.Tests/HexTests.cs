namespace Bitcoin.Tests {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Bitcoin.Core;
	using Xunit;

	public class HexTests {
		[Fact]
		public void HexEncode() {
			var buffer = new byte[] { 0xff, 0xff, 0xff };
			string hex = Hex.Encode(buffer);
			Assert.Equal("ffffff", hex);
		}

		[Fact]
		public void HexDecode() {
			var expected = new byte[] { 0xff, 0xff, 0xff };
			byte[] actual = Hex.Decode("ffffff");
			Assert.Equal(expected, actual);
		}

		[Fact]
		public void HexEncodeNullInput() {
			Assert.Throws<ArgumentNullException>(() => Hex.Encode(null));
		}

		[Fact]
		public void HexDecodeNullInput() {
			Assert.Throws<ArgumentNullException>(() => Hex.Decode(null));
		}
	}
}
