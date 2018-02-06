using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using SqlSugar;
using ToolSetsCore;

namespace SOHO3Q_Alaram
{
    public class DbContextBase
    {
        public SqlSugarClient Db;

        public DbContextBase(string server)
        {
            Db = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = server,
                DbType = DbType.MySql,
                IsAutoCloseConnection = true
            });
        }
    }
}