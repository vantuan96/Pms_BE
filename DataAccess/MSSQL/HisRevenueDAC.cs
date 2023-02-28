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
    public class HisRevenueDAC : IDisposable
    {
        #region Initialization

        private string _ConnectionString = "";
        private bool disposedValue;
        public HisRevenueDAC(string strConnectionString)
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
        /// <summary>
        /// Load data by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IDataReader GetById(int id)
        {
            IDataReader myReader = null;
            using (var da = new SqlDataAccess(_ConnectionString))
            {
                da.AutoCloseConnection = false;
                //Sore
                const string strSql = "PKGACCOUNT.AdsSet_select_item";

                using (var command = (SqlCommand)da.CreateCommand(
                    strSql, CommandType.StoredProcedure, null))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    myReader = (IDataReader)da.ExecuteReader(command,
                       da.CreateParameter("v_id", id, DbType.Int32)
                       //,
                       //da.CreateParameter("returnds", OracleType.Cursor, ParameterDirection.Output)
                       );
                }
            }

            return myReader;
        }
        public IDataReader GetById2(long id)
        {
            IDataReader myReader = null;
            using (var da = new SqlDataAccess(_ConnectionString))
            {
                da.AutoCloseConnection = false;
                //Sore
                const string strSql = "[PKGACCOUNT].[AdsSet_select_item_byAdsSetId]";

                using (var command = (SqlCommand)da.CreateCommand(
                    strSql, CommandType.StoredProcedure, null))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    myReader = (IDataReader)da.ExecuteReader(command,
                       da.CreateParameter("v_adsetId", id, DbType.Int64)
                       //,
                       //da.CreateParameter("returnds", OracleType.Cursor, ParameterDirection.Output)
                       );
                }
            }

            return myReader;
        }

        /// <summary>
        /// Get HISCharge 4 Update Dims
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public IDataReader GetHISCharge4UpdateDims(string fromDate, string toDate)
        {
            IDataReader myReader = null;
            using (var da = new SqlDataAccess(_ConnectionString))
            {
                da.AutoCloseConnection = false;
                const string strSql = "dbo.[GetHISCharge4UpdateDims]";
                using (var command = (SqlCommand)da.CreateCommand(
                    strSql, CommandType.StoredProcedure, null))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    myReader = (IDataReader)da.ExecuteReader(command,
                        da.CreateParameter("v_fDate", fromDate, DbType.String),
                        da.CreateParameter("v_tDate", toDate, DbType.String)
                       );
                }
            }

            return myReader;
        }
        public IDataReader GetHISRevenue4Calculate_VMHC(string visitTypeList, string sStatus4Process, int iTakeCount)
        {
            IDataReader myReader = null;
            using (var da = new SqlDataAccess(_ConnectionString))
            {
                da.AutoCloseConnection = false;
                const string strSql = "dbo.[GetHISRevenue4Calculate_VMHC]";
                using (var command = (SqlCommand)da.CreateCommand(
                    strSql, CommandType.StoredProcedure, null))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    myReader = (IDataReader)da.ExecuteReader(command,
                        da.CreateParameter("v_visittype", visitTypeList, DbType.String),
                        da.CreateParameter("v_status4get", sStatus4Process, DbType.String),
                        da.CreateParameter("v_rownumber", iTakeCount, DbType.Int32)
                       );
                }

            }

            return myReader;
        }
        /// <summary>
        /// Insert Ads Set object
        /// </summary>
        /// <param name="iChannelAdsType"></param>
        /// <param name="sCustomerId"></param>
        /// <param name="sCampaignId"></param>
        /// <param name="sAdSetId"></param>
        /// <param name="sAdSetName"></param>
        /// <param name="sBillingEvent"></param>
        /// <param name="dBudgetRemain"></param>
        /// <param name="dDailyBudget"></param>
        /// <param name="dCreateDate"></param>
        /// <param name="dStartDate"></param>
        /// <param name="dEndDate"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int Create(int iChannelAdsType, string sCustomerId, string sCampaignId, string sAdSetId, string sAdSetName, string sBillingEvent, double dBudgetRemain, double dDailyBudget, DateTime? dCreateDate, DateTime? dStartDate, DateTime? dEndDate, int status)
        {
            var returnValue = -1;

            using (var da = new SqlDataAccess(_ConnectionString))
            {
                const string strSql = "PKGACCOUNT.AdsSet_insert_item";

                using (var command = (SqlCommand)da.CreateCommand(
                    strSql, CommandType.StoredProcedure, null))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    returnValue = Convert.ToInt32(da.ExecuteNonQuery(command,
                         da.CreateParameter("v_channeltype", iChannelAdsType, DbType.Int32, ParameterDirection.Input),
                         da.CreateParameter("v_campaignadsid", sCampaignId, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_adssetid", sAdSetId, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_customerid", sCustomerId, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_adsetname", sAdSetName, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_billing_event", sBillingEvent, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_budget_remaining", dBudgetRemain, DbType.Double, ParameterDirection.Input),
                         da.CreateParameter("v_daily_budget", dDailyBudget, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_createdate", dCreateDate, DbType.DateTime, ParameterDirection.Input),
                         da.CreateParameter("v_startdate", dStartDate, DbType.DateTime, ParameterDirection.Input),
                         da.CreateParameter("v_enddate", dEndDate, DbType.DateTime, ParameterDirection.Input),
                         da.CreateParameter("v_status", status, DbType.Int32, ParameterDirection.Input),
                         da.CreateParameter("v_id", DbType.Int32, ParameterDirection.Output))
                  );
                    int.TryParse(command.Parameters["v_id"].Value.ToString(), out returnValue);
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Update Ads Set Object
        /// </summary>
        /// <param name="id"></param>
        /// <param name="iChannelAdsType"></param>
        /// <param name="sCustomerId"></param>
        /// <param name="sCampaignId"></param>
        /// <param name="sAdSetName"></param>
        /// <param name="sBillingEvent"></param>
        /// <param name="dBudgetRemain"></param>
        /// <param name="dDailyBudget"></param>
        /// <param name="dCreateDate"></param>
        /// <param name="dStartDate"></param>
        /// <param name="dEndDate"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public int Update(int id, int iChannelAdsType, string sCustomerId, string sCampaignId, string sAdSetId, string sAdSetName, string sBillingEvent, double dBudgetRemain, double dDailyBudget, DateTime? dCreateDate, DateTime? dStartDate, DateTime? dEndDate, int status)
        {
            int returnValue = -1;

            using (var da = new SqlDataAccess(_ConnectionString))
            {
                const string strSql = "PKGACCOUNT.AdsSet_update_item";

                using (var command = (SqlCommand)da.CreateCommand(
                    strSql, CommandType.StoredProcedure, null))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    returnValue = Convert.ToInt32(da.ExecuteNonQuery(command,
                        da.CreateParameter("v_id", id, DbType.Int32, ParameterDirection.Input),
                        da.CreateParameter("v_channeltype", iChannelAdsType, DbType.Int32, ParameterDirection.Input),
                         da.CreateParameter("v_campaignadsid", sCampaignId, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_adssetid", sAdSetId, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_customerid", sCustomerId, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_adsetname", sAdSetName, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_billing_event", sBillingEvent, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_budget_remaining", dBudgetRemain, DbType.Double, ParameterDirection.Input),
                         da.CreateParameter("v_daily_budget", dDailyBudget, DbType.String, ParameterDirection.Input),
                         da.CreateParameter("v_createdate", dCreateDate, DbType.DateTime, ParameterDirection.Input),
                         da.CreateParameter("v_startdate", dStartDate, DbType.DateTime, ParameterDirection.Input),
                         da.CreateParameter("v_enddate", dEndDate, DbType.DateTime, ParameterDirection.Input),
                         da.CreateParameter("v_status", status, DbType.Int32, ParameterDirection.Input))
                  );
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Delete one item Ads Set
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public bool Delete(int id)
        {
            int rowsAffected = -1;

            using (var da = new SqlDataAccess(_ConnectionString))
            {
                const string strSql = "PKGACCOUNT.AdsSet_delete";

                using (var command = (SqlCommand)da.CreateCommand(
                    strSql, CommandType.StoredProcedure, null))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    rowsAffected = da.ExecuteNonQuery(command,
                         da.CreateParameter("v_id", id, DbType.Int32)
                    );
                }
            }

            return rowsAffected > 0;
        }
        #endregion
    }
}