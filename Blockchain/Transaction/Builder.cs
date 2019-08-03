using System;
using System.Collections.Generic;
using Blockchain.Exceptions;
using Blockchain.Transactions;
using Blockchain.Utilities;
using TransactionObject = Blockchain.Transaction;

namespace Blockchain
{
    public static class Builder
    {
        public static Func<(string address, ulong amount)[], Func<string, TransactionObject>> Make(SharpKeyPair skp, (TransactionObject tx, int index)[] senderouts)
        {
            List<Input> ins = new List<Input>();

            foreach ((TransactionObject tx, int outindex) in senderouts)
            {
                Output myoutput = tx.GetOutputByIndex(outindex);
                Input txi = new Input
                {
                    Transaction = tx.Id,
                    Index = outindex,
                    Address = skp.GetAddress(),
                    Amount = myoutput.Amount
                };
                txi.Sign(skp);
                ins.Add(txi);
            }

            return ((string address, ulong amount)[] recipients) =>
            {
                List<Output> outs = new List<Output>();

                foreach ((string address, ulong amount) in recipients)
                {
                    Output utxo = new Output
                    {
                        Address = address,
                        Amount = amount
                    };
                    outs.Add(utxo);
                }

                return (string id) =>
                {
                    // Before continuing on we need to check the balance of the transaction
                    // if its postive. Iff thats the case we need to add one more change output
                    // towards the creator of this tx (the skp).

                    TransactionObject newtx = new TransactionObject(ins.ToArray(), outs.ToArray(), id);

                    if (newtx.Balance() > 0)
                    {
                        Output changeout = new Output
                        {
                            Address = skp.GetAddress(),
                            Amount = newtx.Balance()
                        };
                        outs.Add(changeout);
                        newtx = new TransactionObject(ins.ToArray(), outs.ToArray(), id);
                    }

                    newtx.Sign(skp);

                    if (!newtx.Equates() || !newtx.Verify())
                    {
                        throw new BuilderException(newtx, "The constructed transactions did not properly equate or got signed right.");
                    }

                    return newtx;
                };
            };
        }

        public static TransactionObject MakeReward(SharpKeyPair skp, ulong amount, string id = null)
        {
            Output utxo = new Output
            {
                Address = skp.GetAddress(),
                Amount = amount
            };
            Output[] outs = { utxo };
            TransactionObject tx = new TransactionObject(outs, id);
            tx.Sign(skp);
            return tx;
        }
    }
}
