using System;
using System.Collections.Generic;
using Blockchain.Exceptions;
using Blockchain.Transactions;
using Blockchain.Utilities;
using TransactionObject = Blockchain.Transaction;

namespace Blockchain
{
    public class Builder
    {
        private readonly SharpKeyPair skp;
        private readonly List<(TransactionObject, int)> uouts = new List<(TransactionObject, int)>();
        private readonly List<(string, long)> nouts = new List<(string, long)>();

        public Builder(SharpKeyPair skp)
        {
            this.skp = skp;
        }

        public void AddInput(TransactionObject tx, int index)
        {
            uouts.Add((tx, index));
        }

        public void AddOutput(string address, long amount)
        {
            nouts.Add((address, amount));
        }

        public TransactionObject Make()
        {
            return Make(skp, uouts.ToArray())(nouts.ToArray())(null);
        }

        public static Func<(string address, long amount)[], Func<string, TransactionObject>> Make(SharpKeyPair skp, (TransactionObject tx, int index)[] uouts)
        {
            List<Input> ins = new List<Input>();

            foreach ((TransactionObject tx, int outindex) in uouts)
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

                    TransactionObject newtx = new TransactionObject(ins.ToArray(), nouts.ToArray(), id);

                    if (newtx.Balance() > 0)
                    {
                        Output changeout = new Output
                        {
                            Address = skp.GetAddress(),
                            Amount = newtx.Balance()
                        };
                        nouts.Add(changeout);
                        newtx = new TransactionObject(ins.ToArray(), nouts.ToArray(), id);
                    }

                    newtx.Sign(skp);

                    if (!newtx.Equates() || !newtx.Verify())
                    {
                        throw new BuilderException(newtx, "The constructed transaction is not balanced or signed right.");
                    }

                    return newtx;
                };
            };
        }

        public static TransactionObject MakeReward(SharpKeyPair skp, long amount, string id = null)
        {
            TransactionObject tx = new TransactionObject(new Output[] { new Output {
                Address = skp.GetAddress(),
                Amount = amount
            } });
            tx.Sign(skp);
            return tx;
        }
    }
}
