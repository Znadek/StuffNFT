using System;
using System.Text;
using System.ComponentModel;
using System.Numerics;

using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace StuffNFT
{
    [DisplayName("StuffNFT.StuffNFT")]
    [ManifestExtra("Author", "Mattia Braga")]
    [ManifestExtra("Name", "StuffNFT")]
    [ManifestExtra("Email", "mattia.braga.91@gmail.com")]
    [ManifestExtra("Version", "1.0.0")]
    [ManifestExtra("Description", "This NFT represent a stuff of the real world")]
    [SupportedStandards("NEP-11")]
	[ContractPermission("*", "*")]
    public class StuffNFTContract : SmartContract
    {
        private static ByteString Owner() => (ByteString)Storage.Get(Storage.CurrentContext, "Owner");
		private static BigInteger TokenTotal() => (BigInteger)Storage.Get(Storage.CurrentContext, "TokenTotal");
        private static StorageMap AccountAndTokenMap => new StorageMap(Storage.CurrentContext, "AccountAndTokenMap");
        private static StorageMap TokenIdAndTokenValueMap => new StorageMap(Storage.CurrentContext, "TokenIdTokenMap");
        private static StorageMap AddressAndTokenCount => new StorageMap(Storage.CurrentContext, "AddressTokenCount");
		private static Transaction Tx => (Transaction)Runtime.ScriptContainer;

        [DisplayName("Transfer")]
        public static event Action<UInt160, UInt160, BigInteger, ByteString> OnTransfer;

        [Safe]
        [DisplayName("symbol")]
		public static string Symbol() => "STUFF";
        
        [Safe]
        [DisplayName("decimals")]
		public static byte Decimals() => 0;
        
        [Safe]
        [DisplayName("totalSupply")]
		public static BigInteger TotalSupply() => 1000;

        [Safe]
		[DisplayName("balanceOf")]
		public static BigInteger BalanceOf(UInt160 owner)
		{
			IsValidAddress(owner, "owner");
			return (BigInteger)AddressAndTokenCount[owner];
		}
        
        [Safe]
		[DisplayName("tokensOf")]
		public static Iterator tokensOf(UInt160 owner)
		{
            IsValidAddress(owner, "owner");
            return AccountAndTokenMap.Find(owner, FindOptions.KeysOnly | FindOptions.RemovePrefix);
		}

        [Safe]
		[DisplayName("transfer")]
        public static bool Transfer(UInt160 to, ByteString tokenId, object data)
        {
            IsValidAddress(to, "to");
            StuffNFTTokenState token = (StuffNFTTokenState)StdLib.Deserialize(TokenIdAndTokenValueMap[tokenId]);
            UInt160 from = token.Owner;
            if (!Runtime.CheckWitness(from)) return false;
            if (from != to)
            {
                token.Owner = to;
                TokenIdAndTokenValueMap[tokenId] = StdLib.Serialize(token);
                UpdateBalance(from, tokenId, -1);
                UpdateBalance(to, tokenId, +1);
            }
            PostTransfer(from, to, tokenId, data);
            return true;
        }

        public static void MintString(string tokenId, string tokenString)
        {
            StuffNFTTokenState token = (StuffNFTTokenState)StdLib.Deserialize(tokenString);
            Mint((ByteString) tokenId, token);
        }

        public static void Mint(ByteString tokenId, StuffNFTTokenState token)
        {
            TokenIdAndTokenValueMap.Put(tokenId, StdLib.Serialize(token));
            UpdateBalance((UInt160)Tx.Sender, tokenId, +1);
            PostTransfer(null, (UInt160)Tx.Sender, tokenId, null);
        }

        public static void Burn(ByteString tokenId)
        {
            var token = (StuffNFTTokenState)StdLib.Deserialize(TokenIdAndTokenValueMap[tokenId]);
            TokenIdAndTokenValueMap.Delete(tokenId);
            UpdateBalance((UInt160)Tx.Sender, tokenId, -1);
            PostTransfer((UInt160)Tx.Sender, null, tokenId, null);
        }

        [Safe]
        [DisplayName("ownerOf")]
		public static UInt160 OwnerOf(ByteString tokenId)
		{
			return (UInt160)AccountAndTokenMap[tokenId];
		}

        [Safe]
		[DisplayName("tokens")]
		public static Iterator tokens()
		{
			return TokenIdAndTokenValueMap.Find(FindOptions.KeysOnly);
		}

        [Safe]
		[DisplayName("properties")]
		public static string Properties(ByteString tokenId)
		{
			return TokenIdAndTokenValueMap[tokenId];
		}

		[DisplayName("_deploy")]
		public static void Deploy(object data, bool update)
		{
			if (update) return;
			// Initialize contract data
			Storage.Put(Storage.CurrentContext, "Owner", (ByteString)Tx.Sender);
			Storage.Put(Storage.CurrentContext, "TokenTotal", 0);
		}

		public static void Update(ByteString nefFile, string manifest)
		{
			ValidateOwner();
			ContractManagement.Update(nefFile, manifest, null);
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

        private static void IsValidAddress(UInt160 address, string addressDescription = "address")
		{
            if (address is null || !address.IsValid)
                throw new Exception("The argument \"" + addressDescription + "\" is invalid");
		}

        private static void UpdateBalance(UInt160 owner, ByteString tokenId, int increment)
        {
            BigInteger addressTokenCount = (BigInteger)AddressAndTokenCount[owner];
            var tokenTotal = TokenTotal();

			addressTokenCount += increment;
            tokenTotal += increment;
			if (addressTokenCount < 0) throw new Exception("An address cannot have negative token count");

			if (addressTokenCount.IsZero)
				AddressAndTokenCount.Delete(owner);
			else
				AddressAndTokenCount.Put(owner, addressTokenCount);
            
            Storage.Put(Storage.CurrentContext, "TokenTotal", tokenTotal);
        }

        private static void PostTransfer(UInt160 from, UInt160 to, ByteString tokenId, object data)
        {
            OnTransfer(from, to, 1, tokenId);
            if (to is not null && ContractManagement.GetContract(to) is not null)
                Contract.Call(to, "onNEP11Payment", CallFlags.All, from, 1, tokenId, data);
        }
    }
}