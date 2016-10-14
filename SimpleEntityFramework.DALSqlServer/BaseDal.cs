using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Text;
using SimpleEntityFramework.Common;
using SimpleEntityFramework.IDAL;

namespace SimpleEntityFramework.DALSqlServer
{
    public class BaseDal<T> : IBaseDal<T> where T : ModelBase.ModelBase, new()
    {
        #region 0.1 获取连接字符串

        /// <summary>
        ///     连接字符串
        /// </summary>
        private string _connectionStr;

        /// <summary>
        ///     获取连接字符串
        /// </summary>
        /// <returns></returns>
        private string GetConnectionStr()
        {
            if (string.IsNullOrEmpty(_connectionStr))
            {
                _connectionStr = ConfigHelper.GetConnectionString("SqlServerCon");
            }
            return _connectionStr;
        }

        #endregion

        #region 0.3 获取表的字段集合

        /// <summary>
        ///     表的字段集合
        /// </summary>
        private List<string> _colNames;

        /// <summary>
        ///     获取表的字段集合
        /// </summary>
        /// <returns></returns>
        private List<string> GetColNames()
        {
            if (_colNames == null)
            {
                var dataReader = SqlServerHepler.ExecuteReader(GetConnectionStr(), CommandType.Text,
                    "SELECT NAME FROM SYSCOLUMNS WHERE ID=OBJECT_ID('USER');");
                if (dataReader == null)
                {
                    throw new Exception("更新表字段数组出错");
                }
                _colNames = new List<string>();
                while (dataReader.Read())
                {
                    _colNames.Add(dataReader.GetString(0));
                }
            }

            return _colNames;
        }

        #endregion

        #region 0.2 获取表名 ---- 待优化代替这个方法

        //TODO : 待优化代替这个方法

        private string _tbName;

        /// <summary>
        ///     获取表名
        /// </summary>
        /// <returns></returns>
        private string GetTabName()
        {
            if (string.IsNullOrEmpty(_tbName))
            {
                _tbName = "";//new T().TabName;
            }
            return _tbName;
        }

        #endregion

        #region 0.4 将DataReader转为实体集合

        /// <summary>
        ///     将DataReader转化为对象数组
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static List<T> ReaderToModeList(SqlDataReader reader)
        {
            var list = new List<T>();

            while (reader.Read())
            {
                var model = new T();
                var type = model.GetType();

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var property = type.GetProperty(reader.GetName(i));
                    if (property != null)
                    {
                        property.SetValue(model, CheckType(reader[i], property.PropertyType));
                    }
                }
                list.Add(model);
            }

            return list;
        }

        #endregion

        #region 0.5 检查数据类型

        /// <summary>
        ///     对可空类型进行判断转换(*要不然会报错)
        /// </summary>
        /// <param name="value">DataReader字段的值</param>
        /// <param name="conversionType">该字段的类型</param>
        /// <returns></returns>
        private static object CheckType(object value, Type conversionType)
        {
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value == null)
                    return null;
                var nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            }
            return Convert.ChangeType(value, conversionType);
        }

        #endregion

        #region 1.0 通过ID查找实体 - T GetById(int id)

        /// <summary>
        ///     通过ID查找实体
        /// </summary>
        /// <param name="id">查询的ID</param>
        /// <returns></returns>
        public T GetById(int id)
        {
            var dataReader = SqlServerHepler.ExecuteReader(GetConnectionStr(), CommandType.Text,
                string.Format("SELECT * FROM {0} WHERE ID = '{1}'", GetTabName(), id));

            var modelList = ReaderToModeList(dataReader);

            if (modelList.Count == 0)
            {
                throw new Exception(string.Format("没有找到id={0}对应的数据！", id));
            }
            return modelList[0];
            //if (modelList.Count != 1)
            //{
            //    throw new Exception("数据不唯一！");
            //}
        }

        #endregion

        #region 2.0 获取全部实体 - List<T> GetAll()

        /// <summary>
        ///     获取全部实体
        /// </summary>
        /// <returns></returns>
        public List<T> GetAll()
        {
            var dataReader = SqlServerHepler.ExecuteReader(GetConnectionStr(), CommandType.Text,
                string.Format("SELECT * FROM {0}", GetTabName()));
            return ReaderToModeList(dataReader);
        }

        #endregion

        #region 3.0 新增实体 - int Add(T model)

        /// <summary>
        ///     新增实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Add(T model)
        {
            var modelType = model.GetType();
            var sqlBuilder = new StringBuilder();
            var colElementBuilder = new StringBuilder();
            var colValueBuilder = new StringBuilder();
            sqlBuilder.Append("INSERT INTO");
            sqlBuilder.Append(string.Format(" {0} ", GetTabName().ToUpper()));
            sqlBuilder.Append("(");
            foreach (var colName in GetColNames())
            {
                //TODO 存在风险，如果数据库字段名和实体属性名不对应，无法找到，需要添加配置文件或者自动生成实体工具 2016年10月11日
                var property = modelType.GetProperty(colName);
                if (property != null)
                {
                    colElementBuilder.Append(colName);
                    colElementBuilder.Append(",");
                    colValueBuilder.Append(property.GetValue(model));
                    colValueBuilder.Append(",");
                }
            }
            colElementBuilder.Remove(colElementBuilder.Length - 1, colElementBuilder.Length);
            colValueBuilder.Remove(colValueBuilder.Length - 1, colValueBuilder.Length);

            sqlBuilder.Append(colElementBuilder);
            sqlBuilder.Append(") VALUES(");
            sqlBuilder.Append(colValueBuilder);
            sqlBuilder.Append(")");
            return SqlServerHepler.ExecuteNonQuery(GetConnectionStr(), CommandType.Text, sqlBuilder.ToString());
        }

        #endregion

        #region 4.0 根据id删除 - int Del(T model)

        /// <summary>
        ///     根据id删除
        /// </summary>
        /// <param name="model">包含要删除id的对象</param>
        /// <returns></returns>
        public int Del(T model)
        {
            var modelType = model.GetType();
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("DELETE FROM");
            sqlBuilder.Append(string.Format(" {0} ", GetTabName().ToUpper()));
            sqlBuilder.Append(" WHERE ");
            foreach (var colName in GetColNames())
            {
                //TODO 存在风险，如果数据库字段名和实体属性名不对应，无法找到，需要添加配置文件或者自动生成实体工具 2016年10月11日
                var property = modelType.GetProperty(colName);
                if (property != null)
                {
                    sqlBuilder.Append(colName);
                    sqlBuilder.Append("=");
                    sqlBuilder.Append(property.GetValue(model));
                    sqlBuilder.Append(" AND ");
                }
            }
            sqlBuilder.Remove(sqlBuilder.Length - 5, sqlBuilder.Length);
            return SqlServerHepler.ExecuteNonQuery(GetConnectionStr(), CommandType.Text, sqlBuilder.ToString());
        }

        #endregion

        #region 5.0 根据条件删除 - bool DelBy(Func<T,bool> delWhere) ---- 未实现

        /// <summary>
        ///     根据条件删除
        /// </summary>
        /// <param name="delWhere"></param>
        /// <returns></returns>
        public bool DelBy(Func<T, bool> delWhere)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 6.0 修改 - int Modify(T model, params string[] proNames) ---- 未实现

        /// <summary>
        ///     修改
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="proNames">要修改的属性名称</param>
        /// <returns></returns>
        public int Modify(T model, params string[] proNames)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 7.0 批量修改 +int Modify(Func<T,bool> whereLambda, params string[] modifiedProNames) ---- 未实现

        /// <summary>
        ///     批量修改
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="whereLambda">查询条件</param>
        /// <param name="modifiedProNames">要修改的属性名称</param>
        /// <returns></returns>
        public int ModifyBy(T model, Func<T, bool> whereLambda, params string[] modifiedProNames)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 8.0 根据条件查询 +List<T> GetListBy(Func<T,bool> whereLambda)  ---- 未实现

        /// <summary>
        ///     根据条件查询
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public List<T> GetListBy(Func<T, bool> whereLambda)
        {
            throw new NotImplementedException();
        }

        #endregion

      
    }
}