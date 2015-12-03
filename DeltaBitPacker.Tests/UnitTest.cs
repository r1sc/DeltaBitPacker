using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeltaBitPacker.Tests
{
    [TestClass]
    public class UnitTest
    {
        private BitSerializer CreateSerializer()
        {
            var serializer = new BitSerializer();
            serializer.RegisterType(typeof(ServerPacket));
            serializer.RegisterType(typeof(GameState));
            serializer.RegisterType(typeof(Player));
            serializer.RegisterType(typeof(Powerup));
            serializer.RegisterType(typeof(ConquestFlag));
            serializer.RegisterType(typeof(ClientPacket));
            serializer.RegisterType(typeof(ClientState));
            serializer.RegisterType(typeof(Command));
            return serializer;
        }

        [TestMethod]
        public void TestDeltaStream()
        {
            var serializer = CreateSerializer();

            var basePacket = new ServerPacket
            {
                GameState = new GameState()
                {
                    Players = new Player[8],
                    Powerups = new Powerup[8],
                    ConquestFlags = new ConquestFlag[4]
                }
            };
            basePacket.GameState.ConquestFlags[3].Team = 1;
            var serverPacket = basePacket;
            serverPacket.Sequence = 1;
            serverPacket.GameState.CurrentMap = 2;
            serverPacket.GameState.ConquestFlags[0].Team = 1;
            serverPacket.GameState.Players[0].Name = "Dangerous Dave";

            var memstream = new MemoryStream();
            var bitWriter = new BitWriter(memstream);

            serializer.SerializeDelta(bitWriter, basePacket, serverPacket);
            bitWriter.Flush();

            memstream.Position = 0;

            var bitReader = new BitReader(memstream);
            object totallyNewPacket = basePacket;
            serializer.Deserialize(bitReader, true, typeof(ServerPacket), ref totallyNewPacket);

            Assert.AreEqual(serverPacket, totallyNewPacket);
        }

        [TestMethod]
        public void TestFullStream()
        {
            var serializer = CreateSerializer();

            var baseClientPacket = new ClientPacket()
            {
                Commands = new Command[32]
            };
            var clientPacket = baseClientPacket;
            clientPacket.AckSequence = 1;

            var memstream = new MemoryStream();
            var bitWriter = new BitWriter(memstream);

            serializer.Serialize(bitWriter, clientPacket);
            bitWriter.Flush();

            memstream.Position = 0;

            var bitReader = new BitReader(memstream);
            object totallyNewPacket = baseClientPacket;
            serializer.Deserialize(bitReader, false, typeof(ClientPacket), ref totallyNewPacket);

            Assert.AreEqual(clientPacket, totallyNewPacket);
        }
    }
}
