using System;
using System.Net.Sockets;
using System.Linq;
using Core.Utilities;
using Core.P2p;
using System.Text;
using Core.Transactions;
using Core.Exceptions;

namespace Core.TCP
{
    public class CoreServer : AbstractCoreServer
    {
        private readonly ILoggable Log;
        private readonly Serializer serializer = new Serializer();

        public CoreServer(Core core, ILoggable log = null) : base(core, log)
        {
            this.Log = log ?? new NullLogger();
        }

        public override void RequestBlock(TcpClient client, byte[] data)
        {
            if (BitConverter.IsLittleEndian)
                data = data.Reverse().ToArray();

            int index = BitConverter.ToInt32(data, 0);

            try
            {
                byte[] compressedBlock = serializer.Serialize(core.Blockchain.GetBlockByIndex(index) ?? core.Blockchain.GetBlockByIndex(0));

                Log.NewLine($"Sending block {index} to {client.Ip()}.");

                Send(client, Opcodes["RequestBlockResponse"], compressedBlock);
            } catch
            {
                Log.NewLine($"Noop'ed on block request {index} to {client.Ip()}.");
                Send(client, Opcodes["RequestBlockResponse"], Operation.NOOP());
            }
        }

        public override void AcceptBlock(TcpClient client, byte[] data)
        {
            try
            {
                Block block = serializer.Deserialize<Block>(data);
                core.Blockchain.AddBlock(block);

                Log.NewLine($"Accepting block {block.Index} from {client.Ip()}.");

                Send(client, Opcodes["AcceptBlockResponse"], Operation.OK());
            } catch(BlockAssertion ba)
            {
                Log.NewLine($"Rejecting block received from {client.Ip()}. {ba.Message}.");
                Send(client, Opcodes["AcceptBlockResponse"], Operation.NOOP());
            }
        }

        public override void RequestPeers(TcpClient client, byte[] data)
        {
            Log.NewLine($"Sending peers to {client.Ip()}.");
            string peers = PeerManager.GetPeersAsIps().Stringified(",");
            Send(client, Operation.Codes["RequestPeersResponse"], peers);
        }

        public override void AcceptPeers(TcpClient client, byte[] data)
        {
            string[] peers = Encoding.UTF8.GetString(data).Split(",");
            Log.NewLine($"Accepting {peers.Length} new or existing peers from {client.Ip()}.");
            foreach (string peer in peers)
            {
                PeerManager.AddPeer(peer);
            }
        }

        public override void RequestTransaction(TcpClient client, byte[] data)
        {
            string id = Encoding.UTF8.GetString(data);
            Transaction tx = core.Blockchain.GetQueuedTransactionById(id);
            Log.NewLine($"Sending transaction {tx.Id} to {client.Ip()}.");
            Send(client, Operation.Codes["RequestTransactionResponse"], serializer.Serialize(tx));
        }

        public override void AcceptTransaction(TcpClient client, byte[] data)
        {
            Transaction tx = serializer.Deserialize<Transaction>(data);
            Log.NewLine($"Accepting transaction {tx.Id} from {client.Ip()}.");
            core.Blockchain.QueueTransaction(tx);
            Send(client, Operation.Codes["AcceptTransactionResponse"], Operation.OK());
        }
    }
}
