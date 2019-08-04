namespace Core.Transactions
{
    public class Output
    {
        public long Amount = 0;
        public string Address = "";

        public override string ToString()
        {
            return $"{Amount}{Address}";
        }
    }
}
