using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM.Framework.Core;

namespace DataAccess.MSSQL
{
    public class ConfigMasterDataDAC : IDisposable
    {
        #region Initialization

        private string _ConnectionString = "";
        private bool disposedValue;
        public ConfigMasterDataDAC(string strConnectionString)
        {
            _ConnectionString = strConnectionString;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool disposing)
        {
            if ((!disposedValue && disposing))
            {
                //Dosome Dispose;
            }
            disposedValue = true;
        }
        #endregion

        #region Repository Methods
        public bool CheckExistIsNotCalculating(string serviceCode, DateTime startDate, DateTime endDate)
        {
            int returnValue = 0;

            using (var da = new SqlDataAccess(_ConnectionString))
            {
                const string strSql = "dbo.CheckExistIsNotCalculating";

                using (var command = (SqlCommand)da.CreateCommand(
                    strSql, CommandType.StoredProcedure, null))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    returnValue = da.ExecuteNonQuery(command,
                         da.CreateParameter("v_serviceCode", serviceCode, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_startDate", startDate, DbType.DateTime, ParameterDirection.Input),
                         da.CreateParameter("v_endDate", endDate, DbType.DateTime, ParameterDirection.Input),
                         da.CreateParameter("v_returnValue", DbType.Int32, ParameterDirection.Output)
                    );
                    int.TryParse(command.Parameters["v_returnValue"].Value.ToString(), out returnValue);
                }
            }

            return returnValue > 0;
        }
        public bool HisRevenueIshaveChild(string chargeId)
        {
            int returnValue = 0;

            using (var da = new SqlDataAccess(_ConnectionString))
            {
                const string strSql = "dbo.IshaveChild";

                using (var command = (SqlCommand)da.CreateCommand(
                    strSql, CommandType.StoredProcedure, null))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    returnValue = da.ExecuteNonQuery(command,
                         da.CreateParameter("v_chargeId", chargeId, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_returnValue", DbType.Int32, ParameterDirection.Output)
                    );
                    int.TryParse(command.Parameters["v_returnValue"].Value.ToString(), out returnValue);
                }
            }

            return returnValue > 0;
        }
        #endregion
    }
}