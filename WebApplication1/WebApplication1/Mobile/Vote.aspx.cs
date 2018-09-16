using Box.CommonLib.Transaction;
using BoxCommonLib;
using BoxCommonLib.Transactions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace WebApplication1.Mobile
{
    public partial class VotePage : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Init();

        }

        protected new void Init()
        {
            DataTable dt = GetQuestion();
            RadioButtonList list;
            ListItem item;
            Label label;
            Label label_detail;
            foreach (DataRow dr in dt.Rows)
            {
                list = new RadioButtonList();
                label = new Label();
                label_detail = new Label();
                list.Width = 500;
                label.Text = dr["question"].ToString() + "<br />";
                label.CssClass = "title";
                label_detail.Text = dr["q_desc"].ToString();
                list.RepeatDirection = RepeatDirection.Horizontal;
                list.ID = "question" + dr["q_seq_id"].ToString();
                for (int i = 1; i <= 5; i++)
                {
                    item = new ListItem
                    {
                        Text = i.ToString(),
                        Value = i.ToString()
                    };
                    list.Items.Add(item);
                }

                plContainer.Controls.Add(label);
                plContainer.Controls.Add(label_detail);
                plContainer.Controls.Add(list);
            }
        }

        protected DataTable GetQuestion()
        {
            string sql = "select q_seq_id,question,q_desc from question q, question_g g where q.q_group_id = g.q_group_id order by q.q_group_id,q.priority";

            DataTable dt = this.DBC.Select(sql);

            return dt;

        }

        protected void BtnSubmit_Click(object sender, EventArgs e)
        {
            
            //BeginTransaction
            using (IDbTransaction tx = DBC.GetTransaction())
            {
                try
                {
                    BoxTxn boxTxn = new BoxTxn(this.DBC, tx);
                    List<Vote> votes = GetVoteResult();
                    IBoxTransaction voteTx = new VoteTxn(votes);
                    boxTxn.Add(voteTx);
                    boxTxn.DoTransaction(boxTxn.GetTransactionCommands(), tx);
                    boxTxn.Clear();

                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                }

            }

        }

        private List<Vote> GetVoteResult()
        {
            List<Vote> votes = new List<Vote>();
            DataTable dt = GetQuestion();
            string tmp;

            foreach (DataRow dr in dt.Rows)
            {
                tmp = "question" + dr["q_seq_id"].ToString();
                string questionId = dr["q_seq_id"].ToString();
                string value = Request.Form[tmp].ToString();
                if (Request.Form[tmp] == null)
                {
                    throw new Exception(tmp + "none click");
                }
                else
                {
                    Vote vote = new Vote(questionId, value);
                    votes.Add(vote);
                }

            }

            return votes;
        }
    }


    

    


    
}

    

    


