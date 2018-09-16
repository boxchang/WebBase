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
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            string connetionString = null;
            SqlConnection cnn;
            connetionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=evaluate;User ID=sa;Password=123456";
            SqlCommand command;
            SqlDataReader dataReader;
            string sql = "select * from TEST";
            cnn = new SqlConnection(connetionString);

            string tmp = "value";

            try
            {
                
                cnn.Open();

                SqlDataAdapter adpt = new SqlDataAdapter(sql, cnn);
                DataTable dt = new DataTable();
                adpt.Fill(dt);

                tmp = JsonConvert.SerializeObject(dt,Formatting.Indented);

                //command = new SqlCommand(sql, cnn);
                //dataReader = command.ExecuteReader();

                //while (dataReader.Read())
                //{
                //    tmp += dataReader.GetValue(0);
                //}
                //dataReader.Close();
                //command.Dispose();
                cnn.Close();
            }
            catch (Exception ex)
            {

            }
            return tmp;
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
