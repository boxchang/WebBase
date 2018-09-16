using BoxCommonLib;
using BoxCommonLib.Object;
using BoxCommonLib.Transactions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace Box.CommonLib.Transaction
{
    public interface IBoxTransaction
    {
        TransactionType Transaction { get; }
    }

    public enum TransactionType
    {
        VOTE_ADD = 0
    }

    public class BoxTxn : List<IBoxTransaction>
    {
        private DBController cnn;
        private IDbTransaction tx;

        string errorSQL = string.Empty;
        int count = 0;
        public BoxTxn(DBController cnn, IDbTransaction tx)
        {
            this.cnn = cnn;
            this.tx = tx;
        }

        public int DoTransaction(List<IDbCommand> commands, IDbTransaction transaction)
        {

            int num2;
            int num = 0;
            try
            {
                num = this.cnn.DoTransaction(commands, transaction);
                num2 = num;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return num2;
        }

        public IDbCommand GetCommand()
        {
            IDbCommand command = cnn.CreateCommand();
            return command;
        }

        //Commands指令集合
        public List<IDbCommand> GetTransactionCommands()
        {
            List<IDbCommand> list = new List<IDbCommand>();
            IDbCommand command;
            foreach (IBoxTransaction transaction in this)
            {
                switch (transaction.Transaction)
                {
                    case TransactionType.VOTE_ADD:
                        string sqs = DateTime.Now.ToString("YYYYMMDDHHmmss");
                        string sql = "insert into vote1(t_seq_id,t_tch_id,create_date,del_flag) values(@SQS,'1',getdate(),'N')";
                        command = GetCommand();
                        DbCommandExtensionMethods.AddParameter(command, "@SQS", sqs);
                        command.CommandText = sql;
                        list.Add(command);
                        foreach (Vote vote in ((VoteTxn)transaction).votes)
                        {
                            string sql_d = "insert into vote_d(t_seq_id,q_seq_id,score) values(@SQS,@QID,@SCORE)";
                            command = GetCommand();
                            command.CommandText = sql_d;
                            DbCommandExtensionMethods.AddParameter(command, "@SQS", sqs);
                            DbCommandExtensionMethods.AddParameter(command, "@QID", vote.questionId);
                            DbCommandExtensionMethods.AddParameter(command, "@SCORE", vote.value);
                            list.Add(command);
                        }
                        break;
                }
            }
            return list;
        }
    }
    
}

