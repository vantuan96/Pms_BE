using System;
using System.Data;
using System.Data.OracleClient;
using System.Reflection;
using System.Text;
using System.Configuration;
using System.Data.Common;

namespace VM.Framework.Core
{
    /// <summary>
    /// Basic low level Data Access Layer
    /// </summary>
    public class OracleDataAccess : DataAccessBase, IDisposable
    {

        #region Constructor

        public OracleDataAccess()
        {
            this.dbProvider = DbProviderFactories.GetFactory("System.Data.OracleClient");
        }

        public OracleDataAccess(string connectionString)
        {
            this.dbProvider = DbProviderFactories.GetFactory("System.Data.OracleClient");
            this.ConnectionString = connectionString;
        }

        //public OracleDataAccess(string connectionString, string providerName)
        //{
        //    if (providerName == "Westwind.Utilities.Wind Web Request Provider")
        //        this.dbProvider = WebRequestClientFactory.Instance;
        //    else
        //        this.dbProvider = DbProviderFactories.GetFactory(providerName);

        //    this.ConnectionString = connectionString;
        //}

        #endregion


        /// <summary>
        /// Opens a Oracle Connection based on the connection string.
        /// Called internally but externally accessible. Sets the internal
        /// _Connection property.
        /// </summary>
        /// <returns></returns>
        public override bool OpenConnection()
        {
            try
            {
                if (this._Connection == null)
                {
                    if (this.ConnectionString.Contains(";"))
                    {
                        this._Connection = dbProvider.CreateConnection();
                        this._Connection.ConnectionString = this.ConnectionString;
                    }
                    else
                    {
                        // Assume it's a connection string value
                        this._Connection = dbProvider.CreateConnection();
                        this._Connection.ConnectionString = ConfigurationManager.ConnectionStrings[this.ConnectionString].ConnectionString;
                    }
                }

                if (this._Connection.State != ConnectionState.Open)
                    this._Connection.Open();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
                return false;
            }

            return true;
        }


        /// <summary>
        /// Creates a Command object and opens a connection
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="sqlOracle"></param>
        /// <returns></returns>
        public override DbCommand CreateCommand(string sqlOracle, CommandType commandType, params DbParameter[] parameters)
        {
            this.SetError();

            DbCommand command = dbProvider.CreateCommand();
            command.CommandType = commandType;
            command.CommandText = sqlOracle;

            try
            {
                if (this.Transaction != null)
                {
                    command.Transaction = this.Transaction;
                    command.Connection = this.Transaction.Connection;
                }
                else
                {
                    if (!this.OpenConnection())
                        return null;

                    command.Connection = this._Connection;
                }
            }
            catch (Exception ex)
            {
                this.SetError(ex.Message);
                return null;
            }

            if (parameters != null)
            {
                foreach (DbParameter Parm in parameters)
                {
                    command.Parameters.Add(Parm);
                }
            }

            return command;
        }

        
        /// <summary>
        /// Creates a Oracle Parameter for the specific provider
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override DbParameter CreateParameter(string parameterName, object value)
        {
            DbParameter parm = dbProvider.CreateParameter();
            parm.ParameterName = parameterName;
            if (value == null)
                value = DBNull.Value;
            parm.Value = value;
            return parm;
        }


        /// <summary>
        /// Creates a Oracle Parameter for the specific provider
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public OracleParameter CreateParameter(string parameterName, object value, OracleType type)
        {
            OracleParameter parm = (OracleParameter)dbProvider.CreateParameter();
            parm.ParameterName = parameterName;
            parm.Direction = ParameterDirection.Input;
            parm.OracleType = type;
            if (value == null)
                value = DBNull.Value;
            parm.Value = value;
            return parm;
        }


        /// <summary>
        /// Creates a Oracle Parameter for the specific provider
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public OracleParameter CreateParameter(string parameterName, OracleType type, ParameterDirection direction)
        {
            OracleParameter parm = (OracleParameter)dbProvider.CreateParameter();
            parm.ParameterName = parameterName;
            parm.Direction = direction;
            parm.OracleType = type;
            return parm;
        }


        /// <summary>
        /// Creates a Oracle Parameter for the specific provider
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public OracleParameter CreateParameter(string parameterName, OracleType type, int size, ParameterDirection direction)
        {
            OracleParameter parm = (OracleParameter)dbProvider.CreateParameter();
            parm.ParameterName = parameterName;
            parm.Direction = direction;
            parm.OracleType = type;
            parm.Size = size;
            return parm;
        }

        
        /// <summary>
        /// Executes a non-query command and returns the affected records
        /// </summary>
        /// <param name="Command">Command should be created with GetSqlCommand to have open connection</param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public override int ExecuteNonQuery(DbCommand Command, params DbParameter[] Parameters)
        {
            this.SetError();

            int RecordCount = 0;

            if (Parameters != null)
            {
                foreach (DbParameter Parameter in Parameters)
                {
                    Command.Parameters.Add(Parameter);
                }
            }

            try
            {
                RecordCount = Command.ExecuteNonQuery();
                if (RecordCount == -1)
                    RecordCount = 0;
            }
            catch (OracleException ex)
            {
                RecordCount = -1;
                this.ErrorMessage = ex.Message;
                this.ErrorNumber = ex.ErrorCode;

                //Day exception ra web app
                throw ex;
            }
            finally
            {
                this.CloseConnection();
            }

            return RecordCount;
        }


        /// <summary>
        /// Executes a Oracle Command object and returns a OracleDataReader object
        /// </summary>
        /// <param name="Command">Command should be created with GetOracleCommand and open connection</param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        /// <returns>A OracleDataReader. Make sure to call Close() to close the underlying connection.</returns>
        public override DbDataReader ExecuteReader(DbCommand Command, params DbParameter[] Parameters)
        {
            this.SetError();
            if (Command == null)
                return null;
            if (Command.Connection == null || Command.Connection.State != ConnectionState.Open)
            {
                if (!this.OpenConnection())
                    return null;

                Command.Connection = this._Connection;
            }
            
            if (Parameters != null)
            {
                foreach (DbParameter Parameter in Parameters)
                {
                    Command.Parameters.Add(Parameter);
                }
            }
            DbDataReader Reader = null;
            try
            {
                Reader = Command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (OracleException ex)
            {
                this.SetError(ex.Message, ex.ErrorCode);
                this.CloseConnection(Command);
                return null;
            }
            return Reader;
        }


        /// <summary>
        /// Returns a DataTable from a Oracle Command string passed in.
        /// </summary>
        /// <param name="Tablename"></param>
        /// <param name="Command"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public override DataTable ExecuteTable(string Tablename, DbCommand Command, params DbParameter[] Parameters)
        {
            this.SetError();
            if (Parameters != null)
            {
                foreach (DbParameter Parameter in Parameters)
                {
                    Command.Parameters.Add(Parameter);
                }
            }

            DbDataAdapter Adapter = dbProvider.CreateDataAdapter();
            Adapter.SelectCommand = Command;

            DataTable dt = new DataTable(Tablename);

            try
            {
                Adapter.Fill(dt);
            }
            catch (Exception ex)
            {
                this.SetError(ex.Message);
                return null;
            }
            finally
            {
                CloseConnection(Command);
            }

            return dt;
        }


        /// <summary>
        /// Returns a DataTable from a Oracle Command string passed in.
        /// </summary>
        /// <param name="Tablename"></param>
        /// <param name="Command"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public override DataSet ExecuteDataSet(DataSet dataSet, string Tablename, DbCommand Command, params DbParameter[] Parameters)
        {

            this.SetError();

            if (dataSet == null)
                dataSet = new DataSet();

            DbDataAdapter Adapter = dbProvider.CreateDataAdapter();
            Adapter.SelectCommand = Command;

            if (this.ExecuteWithSchema)
                Adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

            if (Parameters != null)
            {
                foreach (DbParameter parameter in Parameters)
                {
                    Command.Parameters.Add(parameter);
                }
            }

            DataTable dt = new DataTable(Tablename);

            if (dataSet.Tables.Contains(Tablename))
                dataSet.Tables.Remove(Tablename);

            try
            {
                Adapter.Fill(dataSet, Tablename);
            }
            catch (Exception ex)
            {
                this.SetError(ex.Message);
                return null;
            }
            finally
            {
                CloseConnection(Command);
            }

            return dataSet;
        }


        /// <summary>
        /// Returns a DataTable from a Oracle Command string passed in.
        /// </summary>
        /// <param name="Tablename"></param>
        /// <param name="Command"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public override DataSet ExecuteDataSet(string Tablename, DbCommand Command, params DbParameter[] Parameters)
        {
            return this.ExecuteDataSet(null, Tablename, Command, Parameters);
        }


        /// <summary>
        /// Executes a command and returns a scalar value from it
        /// </summary>
        /// <param name="OracleCommand">A Oracle Command object</param>
        /// <returns>value or null on failure</returns>
        public override object ExecuteScalar(DbCommand Command, params DbParameter[] Parameters)
        {
            this.SetError();

            if (Parameters != null)
            {
                foreach (DbParameter Parameter in Parameters)
                {
                    Command.Parameters.Add(Parameter);
                }
            }

            object Result = null;
            try
            {
                Result = Command.ExecuteScalar();
            }
            catch (OracleException ex)
            {
                this.SetError(ex.Message, ex.ErrorCode);
            }
            finally
            {
                this.CloseConnection();
            }

            return Result;
        }


        /// <summary>
        /// Executes a Oracle command and returns a single value from it.
        /// </summary>
        /// <param name="Oracle">Oracle string to execute</param>
        /// <param name="Parameters">Any named Oracle parameters</param>
        /// <returns>Result value or null. Check ErrorMessage on Null if unexpected</returns>
        public override object ExecuteScalar(string sqlOracle, params DbParameter[] Parameters)
        {
            this.SetError();

            DbCommand Command = CreateCommand(sqlOracle, Parameters);
            if (Command == null)
                return null;

            return this.ExecuteScalar(Command);
        }


        /// <summary>
        /// Closes a connection
        /// </summary>
        /// <param name="Command"></param>
        public override void CloseConnection(DbCommand Command)
        {
            if (this.AutoCloseConnection)
            {
                if (this.Transaction != null)
                    return;

                if (Command.Connection != null &&
                    Command.Connection.State == ConnectionState.Open)
                    Command.Connection.Close();

                this._Connection = null;
            }
        }


        /// <summary>
        /// Closes an active connection. If a transaction is pending the 
        /// connection is held open.
        /// </summary>
        public override void CloseConnection()
        {
            if (this.AutoCloseConnection)
            {
                if (this.Transaction != null)
                    return;

                if (this._Connection != null &&
                    this._Connection.State == ConnectionState.Open)
                    this._Connection.Close();

                this._Connection = null;
            }
        }

    }
}
