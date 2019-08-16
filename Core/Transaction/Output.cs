namespace Core.Transactions
{
    public class Output
    {
        public long Amount = 0;
        public string Address = "";

        public bool Corresponds(Input input)
        {
            return Address == input.Address && Amount == input.Amount;
        }

        public override string ToString()
        {
            return $"{Amount}{Address}";
        }
    }
}
