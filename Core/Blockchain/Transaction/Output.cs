using System;

namespace Core.Transactions
{
    public class Output: IEquatable<Output>
    {
        public long Amount = 0;
        public string Address = "";

        public bool Corresponds(Input input)
        {
            return Address == input.Address && Amount == input.Amount;
        }

        public bool Equals(Output other)
        {
            return other != null && other.Amount == Amount && other.Address == Address;
        }

        public override string ToString()
        {
            return $"{Amount}{Address}";
        }
    }
}
