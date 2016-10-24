﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using SimpleEntityFramework.Models;

namespace SimpleEntityFramework.Test
{
    internal class Program
    {
        public static readonly string ConnectionString =
            "data source=.;initial catalog=AdvancedSeven;user id=sa;pwd=ch902819;";

        public static readonly string DbDatabase = "AdvancedSeven";
        static int? i;
        private static void Main(string[] args)
        {
            //var list =  GenerationHelper.GetGeneratData(ConnectionString, DbDatabase, "","SimpleEntityFramework.ModelBase", "ModelBase");
            //Common.ModelsHelper.ModelsPropertiesHelper.SaveTest();

            //Console.WriteLine((i == null).ToString());
            //var list = Common.ModelsHelper.ModelsPropertiesHelper.GetEntityProperties(
            //     @"D:\Program Files\Microsoft Visual Studio 12.0\Common7\IDE", "User");
            // foreach (var innerPropertyEntity in list)
            // {
            //     Console.WriteLine(innerPropertyEntity.Name);
            // }
            // Console.ReadKey();

            //Console.WriteLine(GetConditionExpression<User>());
           // var ass = Assembly.Load("SimpleEntityFramework.Models");
            var a = Models.ModelsPropertiesHelper.GetPropertiesRoot();
            var list = Common.ModelsHelper.ModelsPropertiesHelperExt.GetEntityProperties(a, "User");
        }

        public static Expression<Func<T, bool>> GetConditionExpression<T>(string[] options, string fieldName)
        {
            ParameterExpression left = Expression.Parameter(typeof(T), "c");//c=>
            Expression expression = Expression.Constant(false);
            foreach (var optionName in options)
            {
                Expression right = Expression.Call
                       (
                          Expression.Property(left, typeof(T).GetProperty(fieldName)),  //c.DataSourceName
                          typeof(string).GetMethod("Contains", new Type[] { typeof(string) }),// 反射使用.Contains()方法                         
                          Expression.Constant(optionName)           // .Contains(optionName)
                       );
                expression = Expression.Or(right, expression);//c.DataSourceName.contain("") || c.DataSourceName.contain("") 
            }
            Expression<Func<T, bool>> finalExpression
                = Expression.Lambda<Func<T, bool>>(expression, new ParameterExpression[] { left });
            return finalExpression;
        }


        /// <summary>
        ///     对可空类型进行判断转换(*要不然会报错)
        /// </summary>
        /// <param name="value">DataReader字段的值</param>
        /// <param name="conversionType">该字段的类型</param>
        /// <returns></returns>
        private static object CheckType(object value, Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                    return null;
                var nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }
            return Convert.ChangeType(value, conversionType);
        }

        /// <summary>
        ///     判断指定对象是否是有效值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool IsNullOrDBNull(object obj)
        {
            return obj == null || obj is DBNull ? true : false;
        }
    }

    /// <summary>
    ///     生成帮助类
    /// </summary>
    public class GenerationHelper
    {
        /// <summary>
        ///     获取生成数据
        /// </summary>
        /// <param name="connectionStr">连接字符串</param>
        /// <param name="databaseName">数据库名</param>
        /// <param name="tableNames">表名</param>
        /// <param name="parentAssemblyName">父类的解决方案名称</param>
        /// <param name="parentName">父类名称</param>
        /// <returns></returns>
        public static List<GenerateData> GetGeneratData(string connectionStr, string databaseName,
            string tableNames = "", string parentAssemblyName = "",
            string parentName = "")
        {
            var ret = new List<GenerateData>();

            var parentPropeties = GetSuperPropertyInfos(parentAssemblyName, parentName);

            var dbTables = DbHelper.GetDbTables(connectionStr, databaseName, tableNames);

            foreach (var tb in dbTables)
            {
                var tbColunms = DbHelper.GetDbColumns(connectionStr, databaseName, tb.TableName);
                ret.Add(PackingSourceToGenerateData(tb.TableName, tbColunms,
                    string.IsNullOrEmpty(parentName) || string.IsNullOrEmpty(parentAssemblyName)
                        ? ""
                        : string.Format("{0}.{1}", parentAssemblyName, parentName), parentPropeties));
            }
            return ret;
        }

        /// <summary>
        /// 获取父类属性
        /// </summary>
        /// <param name="superType"></param>
        /// <returns></returns>
        private static List<PropertyInfo> GetSuperProperty(Type superType)
        {
            if (superType == null)
                return new List<PropertyInfo>();
            return superType.GetProperties().ToList();
        }

        /// <summary>
        /// 获取父类属性
        /// </summary>
        /// <param name="parentAssemblyName">父类的解决方案名称</param>
        /// <param name="parentName">父类名称</param>
        /// <returns></returns>
        private static List<PropertyInfo> GetSuperPropertyInfos(string parentAssemblyName, string parentName)
        {
            var propertyInfos = new List<PropertyInfo>();
            if (!string.IsNullOrEmpty(parentAssemblyName) && !string.IsNullOrEmpty(parentName))
            {
                var superAss = Assembly.Load(parentAssemblyName);
                if (superAss != null)
                {
                    var superType = superAss.GetType(string.Format("{0}.{1}", parentAssemblyName, parentName));
                    propertyInfos = GetSuperProperty(superType);
                }
            }
            return propertyInfos;
        }

        /// <summary>
        /// 把原始数据拼装程GeneratData
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columns">数据列</param>
        /// <param name="parentName">父类名称</param>
        /// <param name="parentPropeties">父类属性</param>
        /// <returns></returns>
        private static GenerateData PackingSourceToGenerateData(string tableName, List<DbColumn> columns, string parentName,
            List<PropertyInfo> parentPropeties)
        {
            var generateData = new GenerateData();
            //先不不判断父类属性是否为空，可能存在父类只为约束使用，没有属性
            if (string.IsNullOrEmpty(parentName))
            {
                generateData.HasParent = true;
                generateData.ParentName = parentName;
                if (columns != null && columns.Count > 0)
                {
                    var matchCol =
                        columns.FindAll(col => parentPropeties.Exists(pro => string.Equals(pro.Name, col.ColumnName)));
                    if (matchCol.Count == parentPropeties.Count)
                    {
                        matchCol.ForEach(col => col.IsHidden = true);
                    }
                    else
                    {
                        generateData.HasParent = false;
                        generateData.ParentName = "";
                    }
                }
            }
            generateData.TableName = tableName;
            generateData.TbColumns = columns;
            return generateData;
        }
    }

    /// <summary>
    /// 生成类数据实体
    /// </summary>
    public class GenerateData
    {
        public string TableName { get; set; }

        public List<DbColumn> TbColumns { get; set; }

        public bool HasParent { get; set; }

        public string ParentName { get; set; }
    }


    public class DbHelper
    {
        #region GetDbTables

        public static List<DbTable> GetDbTables(string connectionString, string database, string tables = null)
        {
            tables = string.Format(" and obj.name in ({0})",
                string.IsNullOrEmpty(tables)
                    ? "select name from sys.tables"
                    : string.Format("'{0}'", tables.Replace(",", "','")));

            #region SQL

            var sql = string.Format(@"SELECT
                                    obj.name tablename,
                                    schem.name schemname,
                                    idx.rows,
                                    CAST
                                    (
                                        CASE 
                                            WHEN (SELECT COUNT(1) FROM sys.indexes WHERE object_id= obj.OBJECT_ID AND is_primary_key=1) >=1 THEN 1
                                            ELSE 0
                                        END 
                                    AS BIT) HasPrimaryKey                                         
                                    from {0}.sys.objects obj 
                                    inner join {0}.dbo.sysindexes idx on obj.object_id=idx.id and idx.indid<=1
                                    INNER JOIN {0}.sys.schemas schem ON obj.schema_id=schem.schema_id
                                    where type='U' {1}
                                    order by obj.name", database, tables);

            #endregion

            var dt = GetDataTable(connectionString, sql);
            return dt.Rows.Cast<DataRow>().Select(row => new DbTable
            {
                TableName = row.Field<string>("tablename"),
                SchemaName = row.Field<string>("schemname"),
                Rows = row.Field<int>("rows"),
                HasPrimaryKey = row.Field<bool>("HasPrimaryKey")
            }).ToList();
        }

        #endregion

        #region GetDbColumns

        public static List<DbColumn> GetDbColumns(string connectionString, string database, string tableName,
            string schema = "dbo")
        {
            #region SQL

            var sql = string.Format(@"
                                    WITH indexCTE AS
                                    (
                                        SELECT 
                                        ic.column_id,
                                        ic.index_column_id,
                                        ic.object_id    
                                        FROM {0}.sys.indexes idx
                                        INNER JOIN {0}.sys.index_columns ic ON idx.index_id = ic.index_id AND idx.object_id = ic.object_id
                                        WHERE  idx.object_id =OBJECT_ID(@tableName) AND idx.is_primary_key=1
                                    )
                                    select
                                    colm.column_id ColumnID,
                                    CAST(CASE WHEN indexCTE.column_id IS NULL THEN 0 ELSE 1 END AS BIT) IsPrimaryKey,
                                    colm.name Name,
                                    systype.name ColumnType,
                                    colm.is_identity IsIdentity,
                                    colm.is_nullable IsNullable,
                                    cast(colm.max_length as int) ByteLength,
                                    (
                                        case 
                                            when systype.name='nvarchar' and colm.max_length>0 then colm.max_length/2 
                                            when systype.name='nchar' and colm.max_length>0 then colm.max_length/2
                                            when systype.name='ntext' and colm.max_length>0 then colm.max_length/2 
                                            else colm.max_length
                                        end
                                    ) CharLength,
                                    cast(colm.precision as int) Precision,
                                    cast(colm.scale as int) Scale,
                                    prop.value Remark
                                    from {0}.sys.columns colm
                                    inner join {0}.sys.types systype on colm.system_type_id=systype.system_type_id and colm.user_type_id=systype.user_type_id
                                    left join {0}.sys.extended_properties prop on colm.object_id=prop.major_id and colm.column_id=prop.minor_id
                                    LEFT JOIN indexCTE ON colm.column_id=indexCTE.column_id AND colm.object_id=indexCTE.object_id                                        
                                    where colm.object_id=OBJECT_ID(@tableName)
                                    order by colm.column_id", database);

            #endregion

            var param = new SqlParameter("@tableName", SqlDbType.NVarChar, 100)
            {
                Value = string.Format("{0}.{1}.{2}", database, schema, tableName)
            };
            var dt = GetDataTable(connectionString, sql, param);
            return dt.Rows.Cast<DataRow>().Select(row => new DbColumn
            {
                ColumnID = row.Field<int>("ColumnID"),
                IsPrimaryKey = row.Field<bool>("IsPrimaryKey"),
                ColumnName = row.Field<string>("Name"),
                ColumnType = row.Field<string>("ColumnType"),
                IsIdentity = row.Field<bool>("IsIdentity"),
                IsNullable = row.Field<bool>("IsNullable"),
                ByteLength = row.Field<int>("ByteLength"),
                CharLength = row.Field<int>("CharLength"),
                Scale = row.Field<int>("Scale"),
                Remark = row["Remark"].ToString(),
                IsHidden = false
            }).ToList();
        }

        #endregion

        #region GetDataTable

        public static DataTable GetDataTable(string connectionString, string commandText, params SqlParameter[] parms)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = commandText;
                command.Parameters.AddRange(parms);
                var adapter = new SqlDataAdapter(command);

                var dt = new DataTable();
                adapter.Fill(dt);

                return dt;
            }
        }

        #endregion
    }

    #region DbTable

    /// <summary>
    ///     表结构
    /// </summary>
    public sealed class DbTable
    {
        /// <summary>
        ///     表名称
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        ///     表的架构
        /// </summary>
        public string SchemaName { get; set; }

        /// <summary>
        ///     表的记录数
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        ///     是否含有主键
        /// </summary>
        public bool HasPrimaryKey { get; set; }
    }

    #endregion

    #region DbColumn

    /// <summary>
    ///     表字段结构
    /// </summary>
    public sealed class DbColumn
    {
        /// <summary>
        ///     字段ID
        /// </summary>
        public int ColumnID { get; set; }

        /// <summary>
        ///     是否主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        ///     字段名称
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        ///     字段类型
        /// </summary>
        public string ColumnType { get; set; }

        /// <summary>
        ///     数据库类型对应的C#类型
        /// </summary>
        public string CSharpType
        {
            get { return SqlServerDbTypeMap.MapCsharpType(ColumnType); }
        }

        /// <summary>
        /// </summary>
        public Type CommonType
        {
            get { return SqlServerDbTypeMap.MapCommonType(ColumnType); }
        }

        /// <summary>
        ///     字节长度
        /// </summary>
        public int ByteLength { get; set; }

        /// <summary>
        ///     字符长度
        /// </summary>
        public int CharLength { get; set; }

        /// <summary>
        ///     小数位
        /// </summary>
        public int Scale { get; set; }

        /// <summary>
        ///     是否自增列
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        ///     是否允许空
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        ///     描述
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        ///     是否隐藏
        /// </summary>
        public bool IsHidden { get; set; }
    }

    #endregion

    #region SqlServerDbTypeMap

    public class SqlServerDbTypeMap
    {
        public static string MapCsharpType(string dbtype)
        {
            if (string.IsNullOrEmpty(dbtype)) return dbtype;
            dbtype = dbtype.ToLower();
            var csharpType = "object";
            switch (dbtype)
            {
                case "bigint":
                    csharpType = "long";
                    break;
                case "binary":
                    csharpType = "byte[]";
                    break;
                case "bit":
                    csharpType = "bool";
                    break;
                case "char":
                    csharpType = "string";
                    break;
                case "date":
                    csharpType = "DateTime";
                    break;
                case "datetime":
                    csharpType = "DateTime";
                    break;
                case "datetime2":
                    csharpType = "DateTime";
                    break;
                case "datetimeoffset":
                    csharpType = "DateTimeOffset";
                    break;
                case "decimal":
                    csharpType = "decimal";
                    break;
                case "float":
                    csharpType = "double";
                    break;
                case "image":
                    csharpType = "byte[]";
                    break;
                case "int":
                    csharpType = "int";
                    break;
                case "money":
                    csharpType = "decimal";
                    break;
                case "nchar":
                    csharpType = "string";
                    break;
                case "ntext":
                    csharpType = "string";
                    break;
                case "numeric":
                    csharpType = "decimal";
                    break;
                case "nvarchar":
                    csharpType = "string";
                    break;
                case "real":
                    csharpType = "Single";
                    break;
                case "smalldatetime":
                    csharpType = "DateTime";
                    break;
                case "smallint":
                    csharpType = "short";
                    break;
                case "smallmoney":
                    csharpType = "decimal";
                    break;
                case "sql_variant":
                    csharpType = "object";
                    break;
                case "sysname":
                    csharpType = "object";
                    break;
                case "text":
                    csharpType = "string";
                    break;
                case "time":
                    csharpType = "TimeSpan";
                    break;
                case "timestamp":
                    csharpType = "byte[]";
                    break;
                case "tinyint":
                    csharpType = "byte";
                    break;
                case "uniqueidentifier":
                    csharpType = "Guid";
                    break;
                case "varbinary":
                    csharpType = "byte[]";
                    break;
                case "varchar":
                    csharpType = "string";
                    break;
                case "xml":
                    csharpType = "string";
                    break;
                default:
                    csharpType = "object";
                    break;
            }
            return csharpType;
        }

        public static Type MapCommonType(string dbtype)
        {
            if (string.IsNullOrEmpty(dbtype)) return Type.Missing.GetType();
            dbtype = dbtype.ToLower();
            var commonType = typeof(object);
            switch (dbtype)
            {
                case "bigint":
                    commonType = typeof(long);
                    break;
                case "binary":
                    commonType = typeof(byte[]);
                    break;
                case "bit":
                    commonType = typeof(bool);
                    break;
                case "char":
                    commonType = typeof(string);
                    break;
                case "date":
                    commonType = typeof(DateTime);
                    break;
                case "datetime":
                    commonType = typeof(DateTime);
                    break;
                case "datetime2":
                    commonType = typeof(DateTime);
                    break;
                case "datetimeoffset":
                    commonType = typeof(DateTimeOffset);
                    break;
                case "decimal":
                    commonType = typeof(decimal);
                    break;
                case "float":
                    commonType = typeof(double);
                    break;
                case "image":
                    commonType = typeof(byte[]);
                    break;
                case "int":
                    commonType = typeof(int);
                    break;
                case "money":
                    commonType = typeof(decimal);
                    break;
                case "nchar":
                    commonType = typeof(string);
                    break;
                case "ntext":
                    commonType = typeof(string);
                    break;
                case "numeric":
                    commonType = typeof(decimal);
                    break;
                case "nvarchar":
                    commonType = typeof(string);
                    break;
                case "real":
                    commonType = typeof(float);
                    break;
                case "smalldatetime":
                    commonType = typeof(DateTime);
                    break;
                case "smallint":
                    commonType = typeof(short);
                    break;
                case "smallmoney":
                    commonType = typeof(decimal);
                    break;
                case "sql_variant":
                    commonType = typeof(object);
                    break;
                case "sysname":
                    commonType = typeof(object);
                    break;
                case "text":
                    commonType = typeof(string);
                    break;
                case "time":
                    commonType = typeof(TimeSpan);
                    break;
                case "timestamp":
                    commonType = typeof(byte[]);
                    break;
                case "tinyint":
                    commonType = typeof(byte);
                    break;
                case "uniqueidentifier":
                    commonType = typeof(Guid);
                    break;
                case "varbinary":
                    commonType = typeof(byte[]);
                    break;
                case "varchar":
                    commonType = typeof(string);
                    break;
                case "xml":
                    commonType = typeof(string);
                    break;
                default:
                    commonType = typeof(object);
                    break;
            }
            return commonType;
        }
    }

    #endregion
}