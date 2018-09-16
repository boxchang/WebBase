using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace BoxCommonLib
{
    public class PageBase : Page
    {
        protected string FunctionRightName;
        private static readonly string DBControllerObject = "DBControllerObject";

        protected DBController DBC
        {
            get
            {
                return this.dbc;
            }
        }

        private DBController dbc
        {
            get
            {
                if (Application[DBControllerObject] == null)
                    Application.Add(DBControllerObject, new DBController());
                DBController _dbc = (DBController)Application.Get(DBControllerObject);
                if (_dbc.DatabaseConnectionState == null)
                    throw new Exception(Message.IllegalRights);
                if (_dbc.DatabaseConnectionState != ConnectionState.Open)
                    _dbc.OpenConnection();
                return _dbc;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            this.FunctionRightName = base.GetType().BaseType.Name;
            base.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            base.OnInit(e);
        }
    }
}
