namespace Bitcoin.Tests {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Numerics;
	using System.Text;
	using System.Threading.Tasks;
	using Bitcoin.Core;
	using NUnit.Framework;

	[TestFixture]
	public class Base58Tests {
		[Test]
		public void EncodeBigInteger() {
			var buffer = new BigInteger(3471844090L).ToByteArray().ReverseOrder();
			Assert.That(Base58.Encode(buffer), Is.EqualTo("16Ho7Hs"));
		}

		[Test]
		public void EncodeHelloWorld() {
			Assert.That(Base58.Encode(Encoding.ASCII.GetBytes("Hello World")), Is.EqualTo("JxF12TrwUP45BMd"));
		}

		[Test]
		public void EncodeEmptyBuffer() {
			Assert.That(Base58.Encode(new byte[0]), Is.EqualTo(String.Empty));
		}

		[Test]
		public void EncodeNullCharacterBuffers() {
			Assert.That(Base58.Encode(new byte[1]), Is.EqualTo("1"));
			Assert.That(Base58.Encode(new byte[7]), Is.EqualTo("1111111"));
		}

		[Test]
		public void DecodeHelloWorld() {
			Assert.That(Base58.Decode("JxF12TrwUP45BMd"), Is.EqualTo(Encoding.ASCII.GetBytes("Hello World")));
		}

		[Test]
		public void DecodeEmptyString() {
			Assert.That(Base58.Decode(String.Empty), Is.EqualTo(new byte[0]));
		}

		[Test]
		public void DecodeNullCharacterBuffers() {
			Assert.That(Base58.Decode("1"), Is.EqualTo(new byte[1]));
			Assert.That(Base58.Decode("1111"), Is.EqualTo(new byte[4]));
		}

		[Test]
		public void DecodeInvalidBufferThrows() {
			Assert.Throws<AddressFormatException>(() => Base58.Decode("This isn't valid base58"));
		}

		[Test]
		public void DecodeChecked() {
			Base58.DecodeChecked("4stwEBjT6FYyVV");

			Assert.Throws<AddressFormatException>(() => Base58.DecodeChecked("yVv"));
			Assert.Throws<AddressFormatException>(() => Base58.DecodeChecked("4stwEBjT6FYyVv"));

			// Now check we can correctly decode the case where the high bit of the first byte is not zero, so BigInteger
			// sign extends. Fix for a bug that stopped us parsing keys exported using sipas patch.
			Base58.DecodeChecked("93VYUMzRG9DdbRP72uQXjaWibbQwygnvaCu9DumcqDjGybD864T");
		}
	}
}
