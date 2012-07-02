namespace Bitcoin.Tests {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Bitcoin.Core;
	using Xunit;

	public class AddressTests {
		static readonly NetworkParameters testParams = NetworkParameters.testNet();
		static readonly NetworkParameters prodParams = NetworkParameters.prodNet();

		public void stringification() {
			// Test a testnet address.
			Address a = new Address(testParams, Hex.Decode("fda79a24e50ff70ff42f7d89585da5bd19d9e5cc"));
			Assert.Equal("n4eA2nbYqErp7H6jebchxAN59DmNpksexv", a.ToString());

			Address b = new Address(prodParams, Hex.Decode("4a22c3c4cbb31e4d03b15550636762bda0baf85a"));
			Assert.Equal("17kzeh4N8g49GFvdDzSf8PjaPfyoD1MndL", b.ToString());
		}

		[Fact]
		public void AddressDecoding() {
			Address a = new Address(testParams, "n4eA2nbYqErp7H6jebchxAN59DmNpksexv");
			Assert.Equal("fda79a24e50ff70ff42f7d89585da5bd19d9e5cc", Hex.Encode(a.getHash160()));

			Address b = new Address(prodParams, "17kzeh4N8g49GFvdDzSf8PjaPfyoD1MndL");
			Assert.Equal("4a22c3c4cbb31e4d03b15550636762bda0baf85a", Hex.Encode(b.getHash160()));
		}

		[Fact]
		public void AddressErrorPaths() {
			// Check what happens if we try and Decode garbage.
			Assert.Throws<AddressFormatException>(() => new Address(testParams, "this is not a valid address!"));

			// Check the empty case.
			Assert.Throws<AddressFormatException>(() => new Address(testParams, ""));

			// Check the case of a mismatched network.
			try {
				new Address(testParams, "17kzeh4N8g49GFvdDzSf8PjaPfyoD1MndL");
				Assert.True(false, "expected exception not thrown.");
			} catch (WrongNetworkException e) {
				// Success.
				Assert.Equal(NetworkParameters.prodNet().addressHeader, e.VerCode);
				Assert.Equal(NetworkParameters.testNet().acceptableAddressCodes, e.AcceptableVersions);
			}
		}

		[Fact]
		public void AddressGetNetwork() {
			NetworkParameters networkParameters = Address.getParametersFromAddress("17kzeh4N8g49GFvdDzSf8PjaPfyoD1MndL");
			Assert.Equal(NetworkParameters.prodNet().getId(), networkParameters.getId());
			networkParameters = Address.getParametersFromAddress("n4eA2nbYqErp7H6jebchxAN59DmNpksexv");
			Assert.Equal(NetworkParameters.testNet().getId(), networkParameters.getId());
		}
	}
}
