using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using SimpleEntityFramework.Common;
using SimpleEntityFramework.Common.ModelsHelper;
using SimpleEntityFramework.IDAL;
using SimpleEntityFramework.Models;

namespace SimpleEntityFramework.DALSqlServer
{
    public class BaseDal<T> : IBaseDal<T> where T : ModelBase.ModelBase, new()
    {
        #region 1.0 通过ID查找实体 - T GetById(int id)

        /// <summary>
        ///     通过ID查找实体
        /// </summary>
        /// <param name="id">查询的ID</param>
        /// <returns></returns>
        public T GetById(int id)
        {
            var sqlStr = "SELECT {0} FROM [{1}] WHERE ID = @id";
            sqlStr = string.Format(sqlStr,
                string.Join(",", GetFieldEntities().Select(entity => string.Format("[{0}]", entity.Name))),
                typeof (T).Name);
            var dataReader = SqlServerHepler.ExecuteReader(GetConnectionStr(), CommandType.Text, sqlStr,
                new SqlParameter("@id", id));

            var modelList = ReaderToModeList(dataReader);

            return modelList.Count == 0 ? null : modelList[0];
        }

        #endregion

        #region 2.0 获取全部实体 - List<T> GetAll()

        /// <summary>
        ///     获取全部实体
        /// </summary>
        /// <returns></returns>
        public List<T> GetAll()
        {
            var sqlStr = "SELECT {0} FROM [{1}]";
            sqlStr = string.Format(sqlStr,
                string.Join(",", GetFieldEntities().Select(entity => string.Format("[{0}]", entity.Name))),
                typeof (T).Name);
            var dataReader = SqlServerHepler.ExecuteReader(GetConnectionStr(), CommandType.Text, sqlStr);
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
            var sqlStr = "INSERT INTO [{0}] ({1}) VALUES ({2})";
            sqlStr = string.Format(sqlStr, typeof (T).Name,
                string.Join(",",
                    GetFieldEntities()
                        .Where(e => !e.IsPrimaryKey || (e.IsPrimaryKey && !e.IsIdentity))
                        .Select(e => string.Format("[{0}]", e.Name))),
                string.Join(",",
                    GetFieldEntities()
                        .Where(e => !e.IsPrimaryKey || (e.IsPrimaryKey && !e.IsIdentity))
                        .Select(e => string.Format("@{0}", e.Name))));
            var properties = typeof (T).GetProperties();
            var parms =
                typeof (T).GetProperties()
                    .Where(pro =>
                    {
                        var entity = GetFieldEntities().First(e => string.Equals(e.Name, pro.Name));
                        return null != entity && (!entity.IsPrimaryKey || (entity.IsPrimaryKey && !entity.IsIdentity));
                    })
                    .Select(
                        pro => new SqlParameter(string.Format("@{0}", pro.Name), pro.GetValue(model) ?? DBNull.Value))
                    .ToArray();

            return SqlServerHepler.ExecuteNonQuery(GetConnectionStr(), CommandType.Text, sqlStr, parms);
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
            var sqlStr = "DELETE FROM [{0}] WHERE {1}";
            sqlStr = string.Format(sqlStr, typeof (T).Name,
                string.Join(" and ",
                    GetFieldEntities().Where(e => e.IsPrimaryKey).Select(e => string.Format("{0}=@{0}", e.Name))));
            var parms = typeof (T).GetProperties()
                .Where(pro =>
                {
                    var entity = GetFieldEntities().FirstOrDefault(e => string.Equals(e.Name, pro.Name));
                    return null != entity && entity.IsPrimaryKey;
                })
                .Select(pro => new SqlParameter(string.Format("@{0}", pro.Name), pro.GetValue(model) ?? DBNull.Value))
                .ToArray();
            return SqlServerHepler.ExecuteNonQuery(GetConnectionStr(), CommandType.Text, sqlStr, parms);
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
            var delList = GetAll().Where(delWhere).ToList();
            if (delList.Count == 0)
                return true;
            var sqlStr = "DELETE FROM [{0}] WHERE {1}";
            sqlStr = string.Format(sqlStr, typeof (T).Name,
                string.Join(" and ",
                    GetFieldEntities().Where(e => e.IsPrimaryKey).Select(e => string.Format("{0}=@{0}", e.Name))));
            var sqls = new List<string>();
            var paramsList = new List<SqlParameter[]>();

            foreach (var model in delList)
            {
                paramsList.Add(typeof (T).GetProperties()
                    .Where(pro =>
                    {
                        var entity = GetFieldEntities().FirstOrDefault(e => string.Equals(e.Name, pro.Name));
                        return null != entity && entity.IsPrimaryKey;
                    })
                    .Select(
                        pro => new SqlParameter(string.Format("@{0}", pro.Name), pro.GetValue(model) ?? DBNull.Value))
                    .ToArray());
                sqls.Add(sqlStr);
            }
            return SqlServerHepler.ExecuteTransaction(GetConnectionStr(), sqls, paramsList);
        }

        #endregion

        #region 6.0 修改 - int Modify(T model, params string[] proNames) 

        /// <summary>
        ///     修改
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="proNames">要修改的属性名称</param>
        /// <returns></returns>
        public int Modify(T model, params string[] proNames)
        {
            var sqlStr = "update [{0}] set {1} where {2}";
            var modifyCondition = string.Join(",",
                GetFieldEntities()
                    .Where(
                        e =>
                            !e.IsPrimaryKey && (null == proNames || proNames.Length == 0 || proNames.Contains(e.Name)))
                    .Select(e => string.Format("[{0}]=@{0}", e.Name)));
            sqlStr = string.Format(sqlStr, typeof (T).Name, modifyCondition,
                string.Join(",",
                    GetFieldEntities().Where(e => e.IsPrimaryKey).Select(e => string.Format("[{0}]=@{0}", e.Name))));
            var paras = typeof (T).GetProperties()
                .Where(pro =>
                {
                    var entity = GetFieldEntities().FirstOrDefault(e => string.Equals(e.Name, pro.Name));
                    return null != entity &&
                           (entity.IsPrimaryKey || null == proNames || proNames.Length == 0 ||
                            proNames.Contains(entity.Name));
                })
                .Select(pro => new SqlParameter(string.Format("@{0}", pro.Name), pro.GetValue(model) ?? DBNull.Value))
                .ToArray();

            return SqlServerHepler.ExecuteNonQuery(GetConnectionStr(), CommandType.Text, sqlStr, paras);
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
            var modifyList = GetAll().Where(whereLambda).ToList();
            if (modifyList.Count == 0)
                return 0;
            var sqlStr = "update [{0}] set {1} where {2}";
            var modifyCondition = string.Join(",",
                GetFieldEntities()
                    .Where(
                        e =>
                            !e.IsPrimaryKey &&
                            (null == modifiedProNames || modifiedProNames.Length == 0 ||
                             modifiedProNames.Contains(e.Name)))
                    .Select(e => string.Format("[{0}]=@{0}", e.Name)));
            sqlStr = string.Format(sqlStr, typeof (T).Name, modifyCondition,
                string.Join(",",
                    GetFieldEntities().Where(e => e.IsPrimaryKey).Select(e => string.Format("[{0}]=@{0}", e.Name))));
            var sqls = new List<string>();
            var paramsList = new List<SqlParameter[]>();
            foreach (var m in modifyList)
            {
                var paras = typeof (T).GetProperties()
                    .Where(pro =>
                    {
                        var entity = GetFieldEntities().FirstOrDefault(e => string.Equals(e.Name, pro.Name));
                        return null != entity && !entity.IsPrimaryKey &&
                               (null == modifiedProNames || modifiedProNames.Length == 0 ||
                                modifiedProNames.Contains(entity.Name));
                    })
                    .Select(
                        pro => new SqlParameter(string.Format("@{0}", pro.Name), pro.GetValue(model) ?? DBNull.Value))
                    .ToList();
                paras.AddRange(typeof (T).GetProperties().Where(pro =>
                {
                    var entity = GetFieldEntities().FirstOrDefault(e => string.Equals(e.Name, pro.Name));
                    return null != entity && entity.IsPrimaryKey;
                }).Select(
                    pro => new SqlParameter(string.Format("@{0}", pro.Name), pro.GetValue(m) ?? DBNull.Value))
                    .ToList());
                sqls.Add(sqlStr);
                paramsList.Add(paras.ToArray());
            }
            return SqlServerHepler.ExecuteTransaction(GetConnectionStr(), sqls, paramsList) ? 1 : 0;
        }

        #endregion

        #region 8.0 根据条件查询 +List<T> GetListBy(Func<T,bool> whereLambda)

        /// <summary>
        ///     根据条件查询
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public List<T> GetListBy(Func<T, bool> whereLambda)
        {
            return GetAll().Where(whereLambda).ToList();
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
                foreach (var pro in type.GetProperties())
                {
                }
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var property = type.GetProperty(reader.GetName(i));
                    if (property != null)
                    {
                        property.SetValue(model, reader[i] is DBNull ? null : reader[i]);
                    }
                }
                list.Add(model);
            }

            return list;
        }

        #endregion

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
        private List<EntityPropertiesEntity> _fieldEntities;

        /// <summary>
        ///     获取表的字段集合
        /// </summary>
        /// <returns></returns>
        private List<EntityPropertiesEntity> GetFieldEntities()
        {
            if (_fieldEntities == null)
            {
                var docment = ModelsPropertiesHelper.GetPropertiesRoot();
                _fieldEntities = ModelsPropertiesHelperExt.GetEntityProperties(docment, typeof (T).Name);
            }

            return _fieldEntities;
        }

        #endregion
    }
}