namespace Core.Transactions
{
    public class MetaOutput : Output
    {
        // obscure naming for maximum compression and low overhead
        public string Transaction;
        public int Index;
    }
}
