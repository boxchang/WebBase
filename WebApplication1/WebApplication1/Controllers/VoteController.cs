using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class VoteController : ApiController
    {

        Models.Vote[] votes = new Models.Vote[] {
            new Models.Vote { teacherName = "Box", score = 10 },
            new Models.Vote { teacherName = "Ariel", score = 20 }
        };

        //取得所有教師的投票資料
        public IEnumerable<Models.Vote> GetAllVote()
        {
            return votes;
        }

        //取得特定教師的投票資料
        public string GetVote(string id)
        {
            string connetionString = null;
            SqlConnection cnn;
            connetionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=evaluate;User ID=sa;Password=123456";

            string sql = @"select question,avg(score) score 
                            from vote1 v, vote_d d, question q where v.t_seq_id = d.t_seq_id and d.q_seq_id = q.q_seq_id
                            group by question";
            cnn = new SqlConnection(connetionString);

            string tmp = string.Empty;

            try
            {

                cnn.Open();

                SqlDataAdapter adpt = new SqlDataAdapter(sql, cnn);
                DataTable dt = new DataTable();
                adpt.Fill(dt);

                foreach(DataRow dr in dt.Rows)
                {
                    tmp += "{ label: \"" + dr["question"].ToString() + "\", y:" + dr["score"].ToString() + "},"; 
                }

                if (tmp.EndsWith(","))
                {
                    tmp = tmp.Substring(0, tmp.Length - 1);
                }

                tmp = "[" + tmp + "]";
                

          //      dataPoints: [

          //          { label: "apple",  y: 10  },
			       // { label: "orange", y: 15  },
			       // { label: "banana", y: 25  },
			       // { label: "mango",  y: 30  },
			       // { label: "grape",  y: 28  }
		        //]

                //tmp = JsonConvert.SerializeObject(dt, Formatting.Indented);

                cnn.Close();
            }
            catch (Exception ex)
            {

            }
            return tmp;
        }
    }

    
}
