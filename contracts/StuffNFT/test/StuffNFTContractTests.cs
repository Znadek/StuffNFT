using System.Collections.Generic;
using System.Linq;
using System.Text;

using FluentAssertions;
using Neo.Assertions;
using Neo.BlockchainToolkit;
using Neo.BlockchainToolkit.Models;
using Neo.BlockchainToolkit.SmartContract;
using Neo.SmartContract;
using Neo.VM;
using NeoTestHarness;
using Xunit;

namespace StuffNFTTests
{
    [CheckpointPath("test/bin/checkpoints/contract-deployed.neoxp-checkpoint")]
    public class StuffNFTContractTests : IClassFixture<CheckpointFixture<StuffNFTContractTests>>
    {
        readonly CheckpointFixture fixture;
        readonly ExpressChain chain;

        public StuffNFTContractTests(CheckpointFixture<StuffNFTContractTests> fixture)
        {
            this.fixture = fixture;
            this.chain = fixture.FindChain("StuffNFTTests.neo-express");
        }


        [Fact]
        public void contract_owner_in_storage()
        {
            var settings = chain.GetProtocolSettings();
            var owner = chain.GetDefaultAccount("owner").ToScriptHash(settings.AddressVersion);

            using var snapshot = fixture.GetSnapshot();

            // check to make sure contract owner stored in contract storage
            var storages = snapshot.GetContractStorages<StuffNFTContract>();
            storages.Count().Should().Be(1);
            storages.TryGetValue("MetadataOwner", out var item).Should().BeTrue();
            item!.Should().Be(owner);
        }

        [Fact]
        public void symbol()
        {
            var settings = chain.GetProtocolSettings();
            var alice = chain.GetDefaultAccount("alice").ToScriptHash(settings.AddressVersion);

            using var snapshot = fixture.GetSnapshot();

            // ExecuteScript converts the provided expression(s) into a Neo script
            // loads them into the engine and executes it 
            using var engine = new TestApplicationEngine(snapshot, settings, alice);

            engine.ExecuteScript<StuffNFTContract>(c => c.symbol());

            engine.State.Should().Be(VMState.HALT);
            engine.ResultStack.Should().HaveCount(1);
            engine.ResultStack.Peek(0).Should().BeTrue(); //BeTrue();
            //engine.ResultStack.Peek(0).Should().BeEquivalentTo("STUFF");
        }

        [Fact]
        public void mint()
        {
            var settings = chain.GetProtocolSettings();
            var alice = chain.GetDefaultAccount("alice").ToScriptHash(settings.AddressVersion);

            using var snapshot = fixture.GetSnapshot();

            // ExecuteScript converts the provided expression(s) into a Neo script
            // loads them into the engine and executes it 
            using var engine = new TestApplicationEngine(snapshot, settings, alice);
            var model = "{ Name : \"Test\", Description : \"Description test\" }";
            var x = Encoding.ASCII.GetBytes(model);
            engine.ExecuteScript<StuffNFTContract>(c => c.mint(Encoding.ASCII.GetBytes("1"), x));

            engine.State.Should().Be(VMState.HALT);
            engine.ResultStack.Should().HaveCount(1);
            //engine.ResultStack.Peek(0).Should().BeEquivalentTo("STUFF");

            var storages = snapshot.GetContractStorages<StuffNFTContract>();
            var contractStorage = storages.StorageMap("TokenIdTokenMap");
            contractStorage.TryGetValue("1", out var item).Should().BeTrue();
            contractStorage.TryGetValue("2", out var item2).Should().BeFalse();
            //item!.Should().Be(model);
        }
        // [Fact]
        // public void can_change_number()
        // {
        //     var settings = chain.GetProtocolSettings();
        //     var alice = chain.GetDefaultAccount("alice").ToScriptHash(settings.AddressVersion);

        //     using var snapshot = fixture.GetSnapshot();

        //     // ExecuteScript converts the provided expression(s) into a Neo script
        //     // loads them into the engine and executes it 
        //     using var engine = new TestApplicationEngine(snapshot, settings, alice);

        //     engine.ExecuteScript<StuffNFTContract>(c => c.changeNumber(42));

        //     engine.State.Should().Be(VMState.HALT);
        //     engine.ResultStack.Should().HaveCount(1);
        //     engine.ResultStack.Peek(0).Should().BeTrue();

        //     // ensure that notification is triggered
        //     engine.Notifications.Should().HaveCount(1);
        //     engine.Notifications[0].EventName.Should().Be("NumberChanged");
        //     engine.Notifications[0].State[0].Should().BeEquivalentTo(alice);
        //     engine.Notifications[0].State[1].Should().BeEquivalentTo(42);

        //     // ensure correct storage item was created 
        //     var storages = snapshot.GetContractStorages<StuffNFTContract>();
        //     var contractStorage = storages.StorageMap("StuffNFTContract");
        //     contractStorage.TryGetValue(alice, out var item).Should().BeTrue();
        //     item!.Should().Be(42);
        // }
    }
}
