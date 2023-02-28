using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Configuration;
using System.Data.Common;
using System.IO;
using VM.Common;
//using GAPIT.MKT.Helpers;


namespace VM.Framework.Core
{
    /// <summary>
    /// Basic low level Data Access Layer
    /// </summary>
    public abstract class DataAccessBase : IDisposable
    {

        #region Properties

        /// <summary>
        /// The internally used dbProvider
        /// </summary>
        public DbProviderFactory dbProvider = null;

        /// <summary>
        /// An error message if a method fails
        /// </summary>
        public virtual string ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; }
        }
        private string _ErrorMessage = string.Empty;

        /// <summary>
        /// Optional error number returned by failed SQL commands
        /// </summary>
        public int ErrorNumber
        {
            get { return _ErrorNumber; }
            set { _ErrorNumber = value; }
        }
        private int _ErrorNumber = 0;


        /// <summary>
        /// ConnectionString for the data access component
        /// </summary>
        public virtual string ConnectionString
        {
            get { return _ConnectionString; }
            set
            {
                _ConnectionString = value;
            }
        }
        private string _ConnectionString = string.Empty;


        /// <summary>
        /// A SQL Transaction object that may be active. You can 
        /// also set this object to 
        /// </summary>
        public virtual DbTransaction Transaction
        {
            get { return _Transaction; }
            set { _Transaction = value; }
        }
        private DbTransaction _Transaction = null;


        /// <summary>
        /// The SQL Connection object used for connections
        /// </summary>
        public virtual DbConnection Connection
        {
            get { return _Connection; }
            set { _Connection = value; }
        }
        protected DbConnection _Connection = null;

        /// <summary>
        /// Determines whether extended schema information is returned for 
        /// queries from the server. Useful if schema needs to be returned
        /// as part of DataSet XML creation 
        /// </summary>
        public virtual bool ExecuteWithSchema
        {
            get { return _ExecuteWithSchema; }
            set { _ExecuteWithSchema = value; }
        }
        private bool _ExecuteWithSchema = false;
        

        /// <summary>
        /// Close connection automatically after an execution.
        /// Some case this value is false allow processing values before close connection
        /// </summary>
        public virtual bool AutoCloseConnection
        {
            get { return _AutoCloseConnection; }
            set { _AutoCloseConnection = value; }
        }
        private bool _AutoCloseConnection = true;
        
        #endregion


        #region Connection Manager


        /// <summary>
        /// Opens a Sql Connection based on the connection string.
        /// Called internally but externally accessible. Sets the internal
        /// _Connection property.
        /// </summary>
        /// <returns></returns>
        public abstract bool OpenConnection();


        /// <summary>
        /// Closes a connection
        /// </summary>
        /// <param name="Command"></param>
        public abstract void CloseConnection(DbCommand Command);


        /// <summary>
        /// Closes an active connection. If a transaction is pending the 
        /// connection is held open.
        /// </summary>
        public abstract void CloseConnection();


        #endregion


        #region Create Command

        /// <summary>
        /// Creates a Command object and opens a connection
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract DbCommand CreateCommand(string sql, CommandType commandType, params DbParameter[] parameters);

        /// <summary>
        /// Creates a Command object and opens a connection
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public virtual DbCommand CreateCommand(string sql, params DbParameter[] parameters)
        {
            return this.CreateCommand(sql, CommandType.Text, parameters);
        }       

        #endregion


        #region Create Parameter

        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public abstract DbParameter CreateParameter(string parameterName, object value);


        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, ParameterDirection parameterDirection)
        {
            DbParameter parm = this.CreateParameter(parameterName, value);
            parm.Direction = parameterDirection;
            return parm;
        }


        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, int size, ParameterDirection parameterDirection)
        {
            DbParameter parm = this.CreateParameter(parameterName, value);
            parm.Size = size;
            parm.Direction = parameterDirection;
            return parm;
        }

        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, DbType type, ParameterDirection direction)
        {
            DbParameter parm = this.CreateParameter(parameterName, value);
            parm.DbType = type;
            parm.Direction = direction;
            return parm;
        }


        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, int size)
        {
            DbParameter parm = this.CreateParameter(parameterName, value);
            parm.Size = size;
            return parm;
        }

        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, DbType type)
        {
            DbParameter parm = this.CreateParameter(parameterName, value);
            parm.DbType = type;
            return parm;
        }

        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, DbType type, int size)
        {
            DbParameter parm = this.CreateParameter(parameterName, value);
            parm.DbType = type;
            parm.Size = size;
            return parm;
        }

        /// <summary>
        /// Used to create named parameters to pass to commands or the various
        /// methods of this class.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public virtual DbParameter CreateParameter(string parameterName, object value, DbType type, int size, ParameterDirection direction)
        {
            DbParameter parm = this.CreateParameter(parameterName, value);
            parm.DbType = type;
            parm.Size = size;
            parm.Direction = direction;
            return parm;
        }

        #endregion


        #region ExecuteNonQuery

        /// <summary>
        /// Executes a non-query command and returns the affected records
        /// </summary>
        /// <param name="Command">Command should be created with GetSqlCommand to have open connection</param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public abstract int ExecuteNonQuery(DbCommand Command, params DbParameter[] Parameters);

        /// <summary>
        /// Executes a command that doesn't return any data. The result
        /// returns the number of records affected or -1 on error.
        /// </summary>
        /// <param name="sql">SQL statement as a string</param>
        /// <param name="parameters">Any number of SQL named parameters</param>
        /// <returns></returns>
        /// <summary>
        /// Executes a command that doesn't return a data result. You can return
        /// output parameters and you do receive an AffectedRecords counter.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual int ExecuteNonQuery(string sql, params DbParameter[] parameters)
        {
            return this.ExecuteNonQuery(this.CreateCommand(sql), parameters);
        }

        #endregion


        #region ExecuteReader

        /// <summary>
        /// Executes a SQL Command object and returns a SqlDataReader object
        /// </summary>
        /// <param name="Command">Command should be created with GetSqlCommand and open connection</param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        /// <returns>A SqlDataReader. Make sure to call Close() to close the underlying connection.</returns>
        public abstract DbDataReader ExecuteReader(DbCommand Command, params DbParameter[] Parameters);


        /// <summary>
        /// Executes a SQL command against the server and returns a DbDataReader
        /// </summary>
        /// <param name="sql">Sql String</param>
        /// <param name="parameters">Any SQL parameters </param>
        /// <returns></returns>
        public virtual DbDataReader ExecuteReader(string sql, params DbParameter[] parameters)
        {
            DbCommand command = this.CreateCommand(sql, parameters);
            return this.ExecuteReader(command);
        }


        #endregion


        #region ExecuteTable

        /// <summary>
        /// Returns a DataTable from a Sql Command string passed in.
        /// </summary>
        /// <param name="Tablename"></param>
        /// <param name="Command"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public abstract DataTable ExecuteTable(string Tablename, DbCommand Command, params DbParameter[] Parameters);

        /// <summary>
        /// Returns a DataTable from a Sql Command string passed in.
        /// </summary>
        /// <param name="Tablename"></param>
        /// <param name="ConnectionString"></param>
        /// <param name="Sql"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public virtual DataTable ExecuteTable(string Tablename, string Sql, params DbParameter[] Parameters)
        {
            this.SetError();

            DbCommand Command = CreateCommand(Sql, Parameters);
            if (Command == null)
                return null;

            return this.ExecuteTable(Tablename, Command);
        }

        #endregion


        #region ExecuteDataSet
        /// <summary>
        /// Returns a DataSet/DataTable from a Sql Command string passed in. 
        /// </summary>
        /// <param name="Tablename">The name for the table generated or the base names</param>
        /// <param name="Command"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public abstract DataSet ExecuteDataSet(string Tablename, DbCommand Command, params DbParameter[] Parameters);

        /// <summary>
        /// Executes a SQL command against the server and returns a DataSet of the result
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual DataSet ExecuteDataSet(string tablename, string sql, params DbParameter[] parameters)
        {
            return this.ExecuteDataSet(tablename, this.CreateCommand(sql), parameters);
        }


        /// <summary>
        /// Returns a DataSet from a Sql Command string passed in.
        /// </summary>
        /// <param name="Tablename"></param>
        /// <param name="Command"></param>
        /// <param name="Parameters"></param>
        /// <returns></returns>
        public abstract DataSet ExecuteDataSet(DataSet dataSet, string Tablename, DbCommand Command, params DbParameter[] Parameters);

        /// <summary>
        /// Returns a DataTable from a Sql Command string passed in.
        /// </summary>
        /// <param name="tablename"></param>
        /// <param name="Command"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual DataSet ExecuteDataSet(DataSet dataSet, string tablename, string sql, params DbParameter[] parameters)
        {
            DbCommand Command = this.CreateCommand(sql, parameters);
            if (Command == null)
                return null;

            return this.ExecuteDataSet(dataSet, tablename, Command);
        }


        #endregion


        #region ExecuteScalar

        /// <summary>
        /// Executes a command and returns a scalar value from it
        /// </summary>
        /// <param name="SqlCommand">A SQL Command object</param>
        /// <returns>value or null on failure</returns>
        public abstract object ExecuteScalar(DbCommand Command, params DbParameter[] Parameters);

        /// <summary>
        /// Executes a Sql command and returns a single value from it.
        /// </summary>
        /// <param name="Sql">Sql string to execute</param>
        /// <param name="Parameters">Any named SQL parameters</param>
        /// <returns>Result value or null. Check ErrorMessage on Null if unexpected</returns>
        public virtual object ExecuteScalar(string sql, params DbParameter[] parameters)
        {
            return this.ExecuteScalar(this.CreateCommand(sql, parameters), null);
        }


        #endregion


        #region RunSQLScript

        /// <summary>
        /// Executes a long SQL script that contains batches (GO commands). This code
        /// breaks the script into individual commands and captures all execution errors.
        /// 
        /// If ContinueOnError is false, operations are run inside of a transaction and
        /// changes are rolled back. If true commands are accepted even if failures occur
        /// and are not rolled back.
        /// </summary>
        /// <param name="Script"></param>
        /// <param name="ScriptIsFile"></param>
        /// <returns></returns>
        public bool RunSqlScript(string Script, bool ContinueOnError, bool ScriptIsFile)
        {
            this.SetError();

            if (ScriptIsFile)
            {
                try
                {
                    Script = File.ReadAllText(Script);
                }
                catch (Exception ex)
                {
                    this.ErrorMessage = ex.Message;
                    return false;
                }
            }

            string[] ScriptBlocks = System.Text.RegularExpressions.Regex.Split(Script + "\r\n", "GO\r\n");
            string Errors = "";

            if (!ContinueOnError)
                this.BeginTransaction();

            foreach (string Block in ScriptBlocks)
            {
                if (string.IsNullOrEmpty(Block.TrimEnd()))
                    continue;

                if (this.ExecuteNonQuery(Block) == -1)
                {
                    Errors = this.ErrorMessage + "\r\n";
                    if (!ContinueOnError)
                    {
                        this.RollbackTransaction();
                        return false;
                    }
                }
            }

            if (!ContinueOnError)
                this.CommitTransaction();

            if (string.IsNullOrEmpty(Errors))
                return true;

            this.ErrorMessage = Errors;
            return false;
        }

        #endregion    


        #region Generic Entity
        /// <summary>
        /// Generic routine to retrieve an object from a database record
        /// The object properties must match the database fields.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="Table"></param>
        /// <param name="KeyField"></param>
        /// <param name="fieldsToSkip"></param>
        /// <returns></returns>
        public virtual bool GetEntity(object entity, DbCommand command, string fieldsToSkip)
        {
            this.SetError();

            if (string.IsNullOrEmpty(fieldsToSkip))
                fieldsToSkip = string.Empty;

            DbDataReader Reader = this.ExecuteReader(command);
            if (Reader == null)
                return false;

            if (!Reader.Read())
            {
                Reader.Close();
                this.CloseConnection(command);
                return false;
            }

            Type ObjType = entity.GetType();

            PropertyInfo[] Properties = ObjType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo Property in Properties)
            {
                if (!Property.CanRead || !Property.CanWrite)
                    continue;

                string Name = Property.Name;

                if (fieldsToSkip.IndexOf("," + Name.ToLower() + ",") > -1)
                    continue;

                object Value = null;
                try
                {
                    Value = Reader[Name];
                    if (Value is DBNull)
                        Value = null;
                }
                catch
                {
                    continue;
                }

                Property.SetValue(entity, Value, null);
            }

            Reader.Close();
            this.CloseConnection();

            return true;
        }



        /// <summary>
        /// Generic routine to return an Entity that matches the field names of a 
        /// table exactly.
        /// </summary>
        /// <param name="Entity"></param>
        /// <param name="Table"></param>
        /// <param name="KeyField"></param>
        /// <param name="KeyValue"></param>
        /// <param name="FieldsToSkip"></param>
        /// <returns></returns>
        public virtual bool GetEntity(object Entity, string Table, string KeyField, object KeyValue, string FieldsToSkip)
        {
            this.SetError();

            DbCommand Command = this.CreateCommand("select * from " + Table + " where [" + KeyField + "]=@Key",
                                                    this.CreateParameter("@Key", KeyValue));
            if (Command == null)
                return false;

            return this.GetEntity(Entity, Command, FieldsToSkip);
        }

        /// <summary>
        /// Updates an entity object that has matching fields in the database for each
        /// public property. Kind of a poor man's quick entity update mechanism.
        /// </summary>
        /// <param name="Entity"></param>
        /// <param name="Table"></param>
        /// <param name="KeyField"></param>
        /// <param name="FieldsToSkip"></param>
        /// <returns></returns>
        public virtual bool UpdateEntity(object Entity, string Table, string KeyField, string FieldsToSkip)
        {
            if (FieldsToSkip == null)
                FieldsToSkip = string.Empty;
            else
                FieldsToSkip = "," + FieldsToSkip.ToLower() + ",";

            DbCommand Command = this.CreateCommand(string.Empty);

            Type ObjType = Entity.GetType();

            StringBuilder sb = new StringBuilder();
            sb.Append("update " + Table + " set ");

            PropertyInfo[] Properties = ObjType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo Property in Properties)
            {
                if (!Property.CanRead)
                    continue;

                string Name = Property.Name;

                if (FieldsToSkip.IndexOf("," + Name.ToLower() + ",") > -1)
                    continue;

                object Value = Property.GetValue(Entity, null);

                sb.Append(" [" + Name + "]=@" + Name + ",");

                Command.Parameters.Add(this.CreateParameter("@" + Name, Value));
            }

            object pkValue = ReflectionUtils.GetProperty(Entity, KeyField);

            String CommandText = sb.ToString().TrimEnd(',') + " where " + KeyField + "=@__PK";

            Command.Parameters.Add(this.CreateParameter("@__PK", pkValue));
            Command.CommandText = CommandText;

            bool Result = this.ExecuteNonQuery(Command) > -1;
            this.CloseConnection(Command);

            return Result;
        }

        public virtual bool UpdateEntity(object Entity, string Table, string KeyField, string FieldsToSkip, string FieldsToUpdate)
        {
            if (FieldsToSkip == null)
                FieldsToSkip = string.Empty;
            else
                FieldsToSkip = "," + FieldsToSkip.ToLower() + ",";

            DbCommand Command = this.CreateCommand(string.Empty);

            Type ObjType = Entity.GetType();

            StringBuilder sb = new StringBuilder();
            sb.Append("update " + Table + " set ");

            string[] Fields = FieldsToUpdate.Split(',');
            foreach (string Name in Fields)
            {
                if (FieldsToSkip.IndexOf("," + Name.ToLower() + ",") > -1)
                    continue;

                PropertyInfo Property = ObjType.GetProperty(Name);
                if (Property == null)
                    continue;

                object Value = Property.GetValue(Entity, null);

                sb.Append(" [" + Name + "]=@" + Name + ",");

                Command.Parameters.Add(this.CreateParameter("@" + Name, Value));
            }
            object pkValue = ReflectionUtils.GetProperty(Entity, KeyField);

            String CommandText = sb.ToString().TrimEnd(',') + " where " + KeyField + "=@__PK";

            Command.Parameters.Add(this.CreateParameter("@__PK", pkValue));
            Command.CommandText = CommandText;

            bool Result = this.ExecuteNonQuery(Command) > -1;
            this.CloseConnection(Command);

            return Result;
        }

        /// <summary>
        /// Inserts an object into the database based on its type information.
        /// The properties must match the database structure and you can skip
        /// over fields in the FieldsToSkip list.
        /// </summary>
        /// <param name="Entity"></param>
        /// <param name="Table"></param>
        /// <param name="KeyField"></param>
        /// <param name="FieldsToSkip"></param>
        /// <returns>Scope Identity or Null</returns>
        public object InsertEntity(object Entity, string Table, string FieldsToSkip)
        {
            if (FieldsToSkip == null)
                FieldsToSkip = string.Empty;
            else
                FieldsToSkip = "," + FieldsToSkip.ToLower() + ",";

            DbCommand Command = this.CreateCommand(string.Empty);

            Type ObjType = Entity.GetType();

            StringBuilder FieldList = new StringBuilder();
            StringBuilder DataList = new StringBuilder();
            FieldList.Append("insert " + Table + " (");
            DataList.Append(" values (");

            PropertyInfo[] Properties = ObjType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo Property in Properties)
            {
                if (!Property.CanRead)
                    continue;

                string Name = Property.Name;

                if (FieldsToSkip.IndexOf("," + Name.ToLower() + ",") > -1)
                    continue;

                object Value = Property.GetValue(Entity, null);

                FieldList.Append("[" + Name + "],");
                DataList.Append("@" + Name + ",");

                Command.Parameters.Add(this.CreateParameter("@" + Name, Value == null ? DBNull.Value : Value));
            }

            Command.CommandText = FieldList.ToString().TrimEnd(',') + ") " +
                                 DataList.ToString().TrimEnd(',') + ");select SCOPE_IDENTITY()";

            object Result = this.ExecuteScalar(Command);

            //bool Result = this.ExecuteNonQuery(Command) > -1;           

            this.CloseConnection();

            return Result;
        }

        #endregion


        #region Transaction Manager

        /// <summary>
        /// Starts a new transaction on this connection/instance
        /// </summary>
        /// <returns></returns>
        public virtual bool BeginTransaction()
        {
            if (this._Connection == null)
            {
                this._Connection = dbProvider.CreateConnection();
                this._Connection.ConnectionString = this.ConnectionString;
                this._Connection.Open();
            }

            this.Transaction = this._Connection.BeginTransaction();
            if (this.Transaction == null)
                return false;

            return true;
        }

        /// <summary>
        /// Commits all changes to the database and ends the transaction
        /// </summary>
        /// <returns></returns>
        public virtual bool CommitTransaction()
        {
            if (this.Transaction == null)
            {
                this.SetError("No active Transaction to commit.");
                return false;
            }

            this.Transaction.Commit();
            this.Transaction = null;

            this.CloseConnection();

            return true;
        }

        /// <summary>
        /// Rolls back a transaction
        /// </summary>
        /// <returns></returns>
        public virtual bool RollbackTransaction()
        {
            if (this.Transaction == null)
                return true;

            this.Transaction.Rollback();
            this.Transaction = null;

            this.CloseConnection();

            return true;
        }


        #endregion


        #region Error Manager
        /// <summary>
        /// Sets the error message for the failure operations
        /// </summary>
        /// <param name="Message"></param>
        protected virtual void SetError(string Message, int errorNumber)
        {
            if (string.IsNullOrEmpty(Message))
            {
                this.ErrorMessage = string.Empty;
                this.ErrorNumber = 0;
                return;
            }

            this.ErrorMessage = Message;
            this.ErrorNumber = errorNumber;
        }

        /// <summary>
        /// Sets the error message and error number.
        /// </summary>
        /// <param name="message"></param>
        protected virtual void SetError(string message)
        {
            this.SetError(message, 0);
        }

        /// <summary>
        /// Sets the error message for failure operations.
        /// </summary>
        protected virtual void SetError()
        {
            this.SetError(null);
        }

        #endregion


        #region IDisposable Members

        public void Dispose()
        {
            if (this._Connection != null)
                this.CloseConnection();
        }

        #endregion
    }

}
