using System;
using System.ComponentModel;
using System.Numerics;

using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace StuffNFT
{
    public class StuffNFTTokenState
    {
        public UInt160 Owner;
        public string Name;
        public string Description;
        public string Base64Image;
    }
}