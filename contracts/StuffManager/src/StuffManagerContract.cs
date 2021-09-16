using System;
using System.ComponentModel;
using System.Numerics;

using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace StuffNFT {

    [DisplayName("StuffNFT.StuffManagerContract")]
    [ManifestExtra("Author", "Mattia Braga")]
    [ManifestExtra("Name", "StuffManagerContract")]
    [ManifestExtra("Email", "mattia.braga.91@gmail.com")]
    [ManifestExtra("Version", "1.0.0")]
    [ManifestExtra("Description", "This Manage the StuffNFT")]
    [SupportedStandards("NEP-11")]
	[ContractPermission("*", "*")]
    public class StuffManagerContract : SmartContract
    {
        private static ByteString Owner() => (ByteString)Storage.Get(Storage.CurrentContext, "Owner");

		public static void CreateToken(UInt160 NFTScriptHash, ByteString NFTTokenId, ByteString value)
		{
			string name = (string)Contract.Call(NFTScriptHash, "symbol", CallFlags.All );
            if (name != "STUFF")
                throw new Exception("Token nft non valido");

			Contract.Call(NFTScriptHash, "mint", CallFlags.All , new object[] { NFTTokenId, value});

		}

        public static void Destroy()
		{
			ValidateOwner();
			ContractManagement.Destroy();
		}

        private static void ValidateOwner()
		{
			if (!Runtime.CheckWitness((UInt160)Owner())) throw new Exception("No authorization");
		}

    }
}
