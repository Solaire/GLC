using Logger;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace GameLauncher_Console
{
    /// <summary>
    /// SQL database core implementation as a singleton pattern.
    /// Handle connections, executions and transactions
    /// The acual data read/write queries for Platform or Game objects should be defined elsewhere
    /// 
    /// TODOs: 
    ///     On startup, check if the database exists, if not create it and create any tables/constraints
    ///     Once the DB connection is made, perform a schema version check and apply new schema if necessary
    /// </summary>
    public sealed class CSqlDB
    {
        private const string SQL_DATA_SOURCE = "database.db";
        private static CSqlDB m_instance = new CSqlDB();

        public static CSqlDB Instance
        {
            get
            {
                return m_instance;
            }
        }

        static CSqlDB()
        {

        }

        /// <summary>
        /// Create the connection to the database
        /// </summary>
        private CSqlDB()
        {
            Open();
        }

        /// <summary>
        /// Check if the database connection is open
        /// </summary>
        /// <returns><c>True</c> if open, otherwise <c>false</c></returns>
        public bool IsOpen()
        {
            return (m_sqlite_conn != null) && (m_sqlite_conn.State == System.Data.ConnectionState.Open);
        }

        /// <summary>
        /// Create SQLiteConnection and open the data source
        /// </summary>
        /// <returns>Error code</returns>
        public SQLiteErrorCode Open()
        {
            if (IsOpen())
            {
                return SQLiteErrorCode.Ok;
            }

            bool exists = File.Exists(SQL_DATA_SOURCE);
            m_sqlite_conn = new SQLiteConnection("Data source=" + SQL_DATA_SOURCE + ";Version=3;New=False;Compress=True;");
            try
            {
                CLogger.LogInfo("Connecting to Data Source " + SQL_DATA_SOURCE);
                m_sqlite_conn.Open();
            }
            catch (SQLiteException e)
            {
                CLogger.LogWarn("ERROR: Database connection could not be established: " + e.ResultCode);
                return e.ResultCode;
            }

            if(!exists) // New DB. Apply schema
            {
                string script;
                try
                {
                    script = File.ReadAllText("CreateDB.sql");
                }
                catch (IOException e)
                {
                    CLogger.LogError(e);
                    return SQLiteErrorCode.Unknown; // Use unknown for non-sql issues
                }

                if (script.Length > 0)
                {
                    Execute(script);
                }
            }

            return SQLiteErrorCode.Ok;
        }

        /// <summary>
        /// Execure SQL query
        /// </summary>
        /// <param name="qry">The SQL query</param>
        /// <returns>SQL success/failure status code</returns>
        public SQLiteErrorCode Execute(string qry)
        {
            SQLiteCommand cmd;
            cmd = m_sqlite_conn.CreateCommand();
            cmd.CommandText = qry;
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException e)
            {
                CLogger.LogWarn("Could not execute statement\n" + qry);
                CLogger.LogWarn(e.Message);
                return e.ResultCode;
            }
            return SQLiteErrorCode.Ok;
        }

        /// <summary>
        /// Execure SQL select statement
        /// </summary>
        /// <param name="qry">The SQL query</param>
        /// <param name="dataReader">SQLiteDataReader which contains the SELECT result</param>
        /// <returns>SQL success/failure status code</returns>
        public SQLiteErrorCode ExecuteRead(string qry, out SQLiteDataReader dataReader)
        {
            SQLiteCommand cmd;
            cmd = m_sqlite_conn.CreateCommand();
            cmd.CommandText = qry;
            try
            {
                dataReader = cmd.ExecuteReader();
            }
            catch (SQLiteException e)
            {
                CLogger.LogWarn("Could not execute statement\n" + qry);
                CLogger.LogWarn(e.Message);
                dataReader = null;
                return e.ResultCode;
            }
            return SQLiteErrorCode.Ok;
        }
        private SQLiteConnection m_sqlite_conn;
    }

    /// <summary>
    /// Abstract base class for SQL fields
    /// </summary>
    public abstract class CSqlField
    {
        /// <summary>
        /// Flags to describe the field's part in the query
        /// Will be used during statement construction
        /// </summary>
        public enum QryFlag
        {
            cSelRead  = 0x01,
            cInsWrite = 0x02,
            cUpdWrite = 0x08,
            cWhere    = 0xf0,
        }

        public readonly string  m_columnName;
        public readonly QryFlag m_qryFlag;
        public string           m_value;

        protected CSqlField(string columnName, QryFlag qryFlag)
        {
            m_columnName    = columnName;
            m_qryFlag       = qryFlag;
            m_value         = "";
        }

        public TypeCode Type { get; protected set; }

        public string String 
        {
            get { return m_value;  }
            set { m_value = value; }
        }

        public int Integer
        {
            get
            {
                int i;
                Int32.TryParse(m_value, out i);
                return i;
            }
            set { m_value = value.ToString(); }
        }

        public double Double
        {
            get
            {
                double d;
                double.TryParse(m_value, out d);
                return d;
            }
            set { m_value = value.ToString(); }
        }

        public bool Bool
        {
            get { return m_value == "1"; }
            set
            {
                if(value)
                {
                    m_value = "1";
                }
                else
                {
                    m_value = "0";
                }
            }
        }
    }

    public class CSqlFieldString : CSqlField
    {
        public CSqlFieldString(string columnName, QryFlag qryFlag) : base(columnName, qryFlag)
        {
            Type = TypeCode.String;
            String = "";
        }
    }

    public class CSqlFieldInteger : CSqlField
    {
        public CSqlFieldInteger(string columnName, QryFlag qryFlag) : base(columnName, qryFlag)
        {
            Type = TypeCode.Int32;
            Integer = 0;
        }
    }

    public class CSqlFieldDouble : CSqlField
    {
        public CSqlFieldDouble(string columnName, QryFlag qryFlag) : base(columnName, qryFlag)
        {
            Type = TypeCode.Double;
            Double = 0.0;
        }
    }

    public class CSqlFieldBoolean : CSqlField
    {
        public CSqlFieldBoolean(string columnName, QryFlag qryFlag) : base(columnName, qryFlag)
        {
            Type = TypeCode.Boolean;
            Bool = false;
        }
    }

    /// <summary>
    /// Abstract base class for managing SQL queries
    /// 
    /// TODOs:
    ///     Some extra comments/documentation wouldn't hurt
    ///     Support fot he following:
    ///         <, <=, >=, >, != , IS NULL, IS NOT NULL, IN (multiple params)
    ///         ALL JOINS
    ///         ORDER BY, GROUP BY
    ///     As of now, this stuff is probably not super important and can be added when needed
    /// </summary>
    public abstract class CSqlQry
    {
        private readonly string m_tableName;
        protected Dictionary<string, CSqlField> m_fields;
        private SQLiteDataReader m_selectResult;

        protected CSqlQry(string table)
        {
            m_tableName = table;
            m_selectResult = null;
            m_fields = new Dictionary<string, CSqlField>();
        }

        public void ClearFields()
        {
            m_selectResult = null;
            foreach (KeyValuePair<string, CSqlField> field in m_fields)
            {
                field.Value.String = "";
            }
        }

        /// <summary>
        /// Prepare the main statement body
        /// </summary>
        private string PrepareMainStatement(CSqlField.QryFlag stmtFlag)
        {
            string query = "";
            foreach (KeyValuePair<string, CSqlField> field in m_fields)
            {
                if((field.Value.m_qryFlag & stmtFlag) == 0)
                {
                    continue;
                }
                if (query.Length > 0)
                {
                    query += ", ";
                }
                query += field.Key;
            }
            return query;
        }

        /// <summary>
        /// Prepare values for the INSERT statement
        /// </summary>
        private string PrepareInsertStatement()
        {
            string insertValues = "";
            foreach(KeyValuePair<string, CSqlField> field in m_fields)
            {
                if ((field.Value.m_qryFlag & CSqlField.QryFlag.cInsWrite) == 0)
                {
                    continue;
                }
                if (insertValues.Length > 0)
                {
                    insertValues += ", ";
                }
                if(field.Value.Type == TypeCode.String)
                {
                    insertValues += "'";
                    insertValues += field.Value.String;
                    insertValues += "'";
                }
                else if(field.Value.Type == TypeCode.Double)
                {
                    insertValues += field.Value.Double;
                }
                else // Bool fields are 0/1
                {
                    insertValues += field.Value.Integer;
                }
            }
            return insertValues;
        }

        /// <summary>
        /// Prepare values for the query condition
        /// </summary>
        private string PrepareWhereStatement()
        {
            string whereCondition = "";
            foreach (KeyValuePair<string, CSqlField> field in m_fields)
            {
                if((field.Value.m_qryFlag & CSqlField.QryFlag.cWhere) == 0)
                {
                    continue;
                }
                if(whereCondition.Length > 0)
                {
                    whereCondition += " AND ";
                }
                whereCondition += field.Key;
                whereCondition += " = ";
                if(field.Value.Type == TypeCode.String)
                {
                    whereCondition += "'";
                    whereCondition += field.Value.String;
                    whereCondition += "'";
                }
                else if(field.Value.Type == TypeCode.Double)
                {
                    whereCondition += field.Value.Double;
                }
                else // Bool fields are 0/1
                {
                    whereCondition += field.Value.Integer;
                }
            }
            return whereCondition;
        }

        /// <summary>
        /// Prepare the main statement body
        /// </summary>
        private string PrepareUpdateStatement()
        {
            string update = "";
            foreach (KeyValuePair<string, CSqlField> field in m_fields)
            {
                if ((field.Value.m_qryFlag & CSqlField.QryFlag.cUpdWrite) == 0)
                {
                    continue;
                }
                if (update.Length > 0)
                {
                    update += ", ";
                }
                update += field.Key;
                update += " = ";
                if (field.Value.Type == TypeCode.String)
                {
                    update += "'";
                    update += field.Value.String;
                    update += "'";
                }
                else if (field.Value.Type == TypeCode.Double)
                {
                    update += field.Value.Double;
                }
                else // Bool fields are 0/1
                {
                    update += field.Value.Integer;
                }
            }
            return update;
        }

        /// <summary>
        /// Prepare and execure SELECT statement
        /// </summary>
        /// <returns></returns>
        public SQLiteErrorCode Select()
        {
            m_selectResult = null;
            string mainStmt = PrepareMainStatement(CSqlField.QryFlag.cSelRead);
            string whereCondition = PrepareWhereStatement();

            string query = "SELECT " + mainStmt + " FROM " + m_tableName;
            if(whereCondition.Length > 0)
            {
                query += " WHERE " + whereCondition;
            }
            SQLiteErrorCode err = CSqlDB.Instance.ExecuteRead(query, out m_selectResult);
            if(err == SQLiteErrorCode.Ok)
            {
                if(!Fetch()) // Perform a fetch on the first row, if available
                {
                    return SQLiteErrorCode.NotFound;
                }
            }
            return err;
        }

        /// <summary>
        /// Prepare and execure INSERT statement
        /// </summary>
        /// <returns>SQL success/failure status code</returns>
        public SQLiteErrorCode Insert()
        {
            string mainStmt = PrepareMainStatement(CSqlField.QryFlag.cInsWrite);
            string insertValues = PrepareInsertStatement();

            string query = "INSERT INTO " + m_tableName + " ( " + mainStmt + ") ";
            if(insertValues.Length > 0)
            {
                query += " VALUES (" + insertValues + ") ";
            }
            return CSqlDB.Instance.Execute(query);
        }

        /// <summary>
        /// Prepare and execute UPDATE statement
        /// </summary>
        /// <returns>SQL success/failure status code</returns>
        public SQLiteErrorCode Update()
        {
            string mainStmt = PrepareUpdateStatement();
            string whereCondition = PrepareWhereStatement();

            string query = "UPDATE " + m_tableName + " SET " + mainStmt;
            if (whereCondition.Length > 0)
            {
                query += " WHERE " + whereCondition;
            }
            return CSqlDB.Instance.Execute(query);
        }

        /// <summary>
        /// Prepare and execure DELETE statement
        /// </summary>
        /// <returns>SQL success/failure status code</returns>
        public SQLiteErrorCode Delete()
        {
            m_selectResult = null;
            string whereCondition = PrepareWhereStatement();

            string query = "DELETE FROM " + m_tableName;
            if(whereCondition.Length > 0)
            {
                query += " WHERE " + whereCondition;
            }
            return CSqlDB.Instance.Execute(query);
        }

        /// <summary>
        /// Fetch the next row from the select result
        /// </summary>
        /// <returns><c>false</c>if reached end of results, otherwise <c>true</c></returns>
        public bool Fetch()
        {
            if(m_selectResult == null)
            {
                return false;
            }
            if(!m_selectResult.Read()) // End of results.
            {
                m_selectResult = null;
                ClearFields();
                return false;
            }

            // Get the values
            foreach (KeyValuePair<string, CSqlField> field in m_fields)
            {
                if((field.Value.m_qryFlag & CSqlField.QryFlag.cSelRead) > 0)
                {
                    field.Value.String = m_selectResult[field.Key].ToString();
                }
            }
            return true;
        }
    }
}
