using Logger;
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

    public struct CSqlField
    {
        /// <summary>
        /// Flags to describe the field's value type (string, bool, etc)
        /// </summary>
        public enum FieldType
        {
            cTypeInteger,
            cTypeDouble,
            cTypeString,
            cTypeBit
        }

        /// <summary>
        /// Flags to describe the field's position in the query (part of SELECT, WHERE, etc)
        /// </summary>
        public enum QueryFlag
        {
            cSelRead  = 0x01,
            cInsWrite = 0x02,
            cUpdWrite = 0x08,
            cSelWhere = 0x10,
            cInsWhere = 0x20,
            cUpdWhere = 0x40,
            cDelWhere = 0x80,

            cCondition = 0xf0,
        }

        public string m_columnName;
        public string m_columnValue; // Initial string, will be converted to the appropriate type
        public QueryFlag m_qryFlag;
        public FieldType m_fieldType;

        public CSqlField(string column, QueryFlag flag, FieldType type)
        {
            m_columnName    = column;
            m_columnValue   = "";
            m_qryFlag       = flag;
            m_fieldType     = type;
        }

        public int Int()
        {
            int ret;
            int.TryParse(m_columnValue, out ret);
            return ret;
        }

        public string String()
        {
            return m_columnValue;
        }

        public bool Bool()
        {
            return m_columnValue == "1";
        }
    }

    /// <summary>
    /// Abstract base class for managing SQL queries
    /// </summary>
    public abstract class CSqlQry
    {
        private string m_qry = ""; // Query body
        private string m_selectWhereCondition = ""; // Query condition
        private string m_insertValues = ""; // Values for the insert statement
        private readonly string m_table;

        protected CSqlQry(string table)
        {
            m_table = table;
        }

        /// <summary>
        /// Initialise the query and select condtions
        /// </summary>
        /// <param name="fields">Array of SQL fields</param>
        protected void InitialiseQuery(CSqlField[] fields)
        {
            List<CSqlField> noCondition = new List<CSqlField>(); // Contains all standard fields
            List<CSqlField> condition = new List<CSqlField>(); // Contains all fields which are part of a WHERE clause
            foreach(CSqlField field in fields)
            {
                // Check each field to see if it's conditional (part of WHERE) or not.
                // NOTE: The field can be both
                if((field.m_qryFlag & CSqlField.QueryFlag.cCondition) > 0)
                {
                    condition.Add(field);
                }
                if((field.m_qryFlag & (~CSqlField.QueryFlag.cCondition)) > 0)
                {
                    noCondition.Add(field);
                }
            }

            for(int i = 0; i < noCondition.Count; i++)
            {
                m_qry += noCondition[i].m_columnName;
                if(noCondition[i].m_columnValue.Length > 0)
                {
                    m_qry += " = ";
                    if(noCondition[i].m_fieldType == CSqlField.FieldType.cTypeString)
                    {
                        m_qry += "'";
                        m_qry += noCondition[i].m_columnValue;
                        m_qry += "'";
                    }
                    else
                    {
                        m_qry += noCondition[i].m_columnValue;
                    }
                }
                if(i != noCondition.Count - 1)
                {
                    m_qry += ", ";
                }
            }
        }

        /// <summary>
        /// Execure INSERT query
        /// </summary>
        /// <returns>SQL success/failure status code</returns>
        public virtual SQLiteErrorCode Insert()
        {
            string query = "INSERT INTO " + m_table + " (" + m_qry + ") ";
            if(m_insertValues.Length > 0)
            {
                query += " VALUES (" + m_insertValues + ") ";
            }
            return CSqlDB.Instance.Execute(query);
        }

        /// <summary>
        /// Execure SELECT query
        /// </summary>
        /// <returns>SQL success/failure status code</returns>
        public virtual SQLiteErrorCode Select(CSqlField[] fields)
        {
            string query = "SELECT " + m_qry + " FROM " + m_table;
            if (m_selectWhereCondition.Length > 0)
            {
                query += " WHERE " + m_selectWhereCondition;
            }
            SQLiteDataReader reader;
            SQLiteErrorCode err = CSqlDB.Instance.ExecuteRead(query, out reader);
            if(reader != null && err == SQLiteErrorCode.Ok)
            {
                int limit = 0;
                int col = 0;
                reader.Read(); // Only once for now TODO:
                {
                    limit = reader.FieldCount;
                    for(int i = 0; i < fields.Length; i++)
                    {
                        if(reader.GetName(col) == fields[i].m_columnName)
                        {
                            switch(fields[i].m_fieldType)
                            {
                                case CSqlField.FieldType.cTypeBit:
                                    fields[i].m_columnValue = reader.GetBoolean(col).ToString();
                                    break;

                                case CSqlField.FieldType.cTypeDouble:
                                    fields[i].m_columnValue = reader.GetDouble(col).ToString();
                                    break;

                                case CSqlField.FieldType.cTypeInteger:
                                    fields[i].m_columnValue = reader.GetInt32(col).ToString();
                                    break;

                                case CSqlField.FieldType.cTypeString:
                                    fields[i].m_columnValue = reader.GetString(col);
                                    break;

                                default:
                                    break;
                            }
                            i++;
                            col++;
                        }
                    }
                }
            }
            return err;
        }

        /// <summary>
        /// Execure UPDATE query
        /// </summary>
        /// <returns>SQL success/failure status code</returns>
        public virtual SQLiteErrorCode Update()
        {
            string query = "UPDATE " + m_table + " SET " + m_qry;
            if (m_selectWhereCondition.Length > 0)
            {
                query += " WHERE " + m_selectWhereCondition;
            }
            return CSqlDB.Instance.Execute(query);
        }

        /// <summary>
        /// Execure DELETE query
        /// </summary>
        /// <returns>SQL success/failure status code</returns>
        public virtual SQLiteErrorCode Delete()
        {
            string query = "DELETE FROM" + m_table;
            if (m_selectWhereCondition.Length > 0)
            {
                query += " WHERE " + m_selectWhereCondition;
            }
            return CSqlDB.Instance.Execute(query);
        }

        protected void PrepareInsertStmt(CSqlField[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].m_fieldType == CSqlField.FieldType.cTypeString)
                {
                    m_insertValues += "'";
                    m_insertValues += fields[i].m_columnValue;
                    m_insertValues += "'";
                }
                else
                {
                    m_insertValues += fields[i].m_columnValue;
                }
                if (i != fields.Length - 1)
                {
                    m_insertValues += ", ";
                }
            }
        }

        protected void PrepareSelectStmt(CSqlField[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if ((fields[i].m_qryFlag & (CSqlField.QueryFlag.cCondition)) == 0)
                {
                    continue;
                }

                if (m_selectWhereCondition.Length > 0)
                {
                    m_selectWhereCondition += " AND ";
                }

                m_selectWhereCondition += fields[i].m_columnName;
                if (fields[i].m_columnValue.Length > 0)
                {
                    m_selectWhereCondition += " = ";
                    if (fields[i].m_fieldType == CSqlField.FieldType.cTypeString)
                    {
                        m_selectWhereCondition += "'";
                        m_selectWhereCondition += fields[i].m_columnValue;
                        m_selectWhereCondition += "'";
                    }
                    else
                    {
                        m_selectWhereCondition += fields[i].m_columnValue;
                    }
                }
            }
        }
    }
}
