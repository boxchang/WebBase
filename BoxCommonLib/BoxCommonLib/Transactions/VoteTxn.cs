using Box.CommonLib.Transaction;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace BoxCommonLib.Transactions
{
    public class Vote
    {
        public string questionId { get; set; }
        public string value { get; set; }
        public Vote(string questionId, string value)
        {
            this.questionId = questionId;
            this.value = value;
        }

    }

    public class VoteTxn : IBoxTransaction
    {
        private string transactionName = string.Empty;
        public List<Vote> votes;
        private List<SqlCommand> sqlCommands = new List<SqlCommand>();
        public VoteTxn(List<Vote> votes)
        {
            this.votes = votes;
        }

        public TransactionType Transaction
        {
            get
            {
                return TransactionType.VOTE_ADD;
            }
        }

    }
}
