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
        private readonly ILoggable log;
        private readonly Serializer serializer = new Serializer();

        public CoreServer(Core core, ILoggable log = null) : base(core, log)
        {
            this.log = log ?? new NullLogger();
        }

        public override void RequestBlock(TcpClient client, byte[] data)
        {
            try
            {
                if (BitConverter.IsLittleEndian)
                    data = data.Reverse().ToArray();

                int index = BitConverter.ToInt32(data, 0);
                byte[] compressedBlock = serializer.Serialize(core.Blockchain.GetBlockByIndex(index) ?? core.Blockchain.GetBlockByIndex(0));

                log.NewLine($"Sending block {index} to {client.Client.RemoteEndPoint.ToString()}.");

                Send(client, Opcodes["RequestBlockResponse"], compressedBlock);
            } catch
            {
                Send(client, Opcodes["RequestBlockResponse"], Operation.NOOP());
            }
        }

        public override void AcceptBlock(TcpClient client, byte[] data)
        {
            try
            {
                Block block = serializer.Deserialize<Block>(data);

                core.Blockchain.AddBlock(block);
                Send(client, Opcodes["AcceptBlockResponse"], Operation.OK());
            } catch(BlockAssertion ba)
            {
                log.NewLine(ba.Message);
                Send(client, Opcodes["AcceptBlockResponse"], Operation.NOOP());
            }
        }

        public override void RequestPeers(TcpClient client, byte[] data)
        {
            string peers = PeerManager.GetPeersAsIps().Reduce(R.Concat(","), "");
            Send(client, Operation.Codes["RequestPeersResponse"], peers);
        }

        public override void AcceptPeers(TcpClient client, byte[] data)
        {
            string[] peers = Encoding.UTF8.GetString(data).Split(",");
            foreach (string peer in peers)
            {
                PeerManager.AddPeer(peer);
            }
        }

        public override void RequestTransaction(TcpClient client, byte[] data)
        {
            string id = Encoding.UTF8.GetString(data);
            Transaction tx = core.Blockchain.GetQueuedTransactionById(id);
            Send(client, Operation.Codes["RequestTransactionResponse"], serializer.Serialize(tx));
        }

        public override void AcceptTransaction(TcpClient client, byte[] data)
        {
            Transaction tx = serializer.Deserialize<Transaction>(data);
            core.Blockchain.QueueTransaction(tx);
            Send(client, Operation.Codes["AcceptTransactionResponse"], Operation.OK());
        }
    }
}
