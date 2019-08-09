using System;
using System.Linq;
using Core.Utilities;

namespace Core.TCP
{
    public class CoreClient : AbstractCoreClient
    {
        private readonly Serializer serializer = new Serializer();

        public CoreClient(Core core, string server) : base(core, server) { }

        public override void AcceptBlock(Block block)
        {
            Send(Operation.Codes["AcceptBlock"], serializer.Serialize(block));
        }

        public override void AcceptBlockResponse(byte[] data) { }

        public override void RequestBlock(int index)
        {
            Send(Operation.Codes["RequestBlock"], BitConverter.GetBytes(index).Reverse().ToArray());
        }

        public override void RequestBlockResponse(byte[] data)
        {
            if (data != NOOP())
            {
                Block block = serializer.Deserialize<Block>(data);
                Console.WriteLine($"Received block: {block.Index}");
                //core.bc.AddBlock(block);
            }
        }
    }
}
