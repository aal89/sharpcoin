using System;
using System.Collections.Generic;
using Core.Crypto;
using Core.Exceptions;

namespace Core.Transactions
{
    public class Builder
    {
        private readonly SharpKeyPair skp;
        private readonly List<(Transaction, int)> uouts = new List<(Transaction, int)>();
        private readonly List<(string, long)> nouts = new List<(string, long)>();

        private Transaction Product;

        public Builder(SharpKeyPair skp)
        {
            this.skp = skp;
        }

        public void AddInput(Transaction tx, int index)
        {
            uouts.Add((tx, index));
        }

        public void AddOutput(string address, long amount)
        {
            nouts.Add((address, amount));
        }

        public Transaction Make()
        {
            return Make(skp, uouts.ToArray())(nouts.ToArray())(null);
        }

        public Func<(string address, long amount)[], Func<string, Transaction>> Make(SharpKeyPair skp, (Transaction tx, int index)[] uouts)
        {
            if (Product != null)
                throw new BuilderException(Product, "Builder has already built a transaction. To build a new transaction, create a new builder first.");

            List<Input> ins = new List<Input>();

            foreach ((Transaction tx, int outindex) in uouts)
            {
                Output myoutput = tx.GetOutputByIndex(outindex);
                Input txi = new Input
                {
                    Transaction = tx.Id,
                    Index = outindex,
                    Address = myoutput.Address,
                    Amount = myoutput.Amount
                };
                txi.Sign(skp);
                ins.Add(txi);
            }

            return ((string address, long amount)[] recipients) =>
            {
                List<Output> nouts = new List<Output>();

                foreach ((string address, long amount) in recipients)
                {
                    Output utxo = new Output
                    {
                        Address = address,
                        Amount = amount
                    };
                    nouts.Add(utxo);
                }

                return (string id) =>
                {
                    // Before continuing on we need to check the balance of the transaction
                    // if its postive. Iff thats the case we need to add one more change output
                    // towards the creator of this tx (the skp).

                    Transaction newtx = new Transaction(ins.ToArray(), nouts.ToArray(), id);

                    if (newtx.Balance() > 0)
                    {
                        Output changeout = new Output
                        {
                            Address = skp.GetAddress(),
                            Amount = newtx.Balance()
                        };
                        nouts.Add(changeout);
                        newtx = new Transaction(ins.ToArray(), nouts.ToArray(), id);
                    }

                    newtx.Sign(skp);

                    if (!newtx.Equates() || !newtx.Verify())
                    {
                        throw new BuilderException(newtx, "The constructed transaction is not balanced or signed right.");
                    }

                    Product = newtx;

                    return Product;
                };
            };
        }

        public static Transaction MakeReward(SharpKeyPair skp, long amount, string id = null)
        {
            Transaction tx = new Transaction(new Output[] { new Output {
                Address = skp.GetAddress(),
                Amount = amount
            } });
            tx.Sign(skp);
            return tx;
        }
    }
}
