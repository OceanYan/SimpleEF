using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace SimpleEntityFramework.Test
{
    /// <summary>
    /// 数据库使用公共类
    /// </summary>
    public class SqlHelper1
    {
        public static string Connection
        {
            get
            {
                return SimpleEntityFramework.Common.ConfigHelper.GetConnectionString("SqlConnection");
            }
        }

        /// <summary>
        /// 增删改数据
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public static bool Processing(string strSql)
        {
            using (SqlConnection sqlCon = new SqlConnection(Connection))
            {
                sqlCon.Open();
                SqlCommand sqlCom = new SqlCommand(strSql, sqlCon);
                if (sqlCom.ExecuteNonQuery() > 0)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 指定条件查询，返回实体集合
        /// </summary>
        /// <returns></returns>
        public static List<T> GetTabaleValue<T>(string strSql)
            where T : class,new()
        {
            using (SqlConnection sqlCon = new SqlConnection(SqlHelper1.Connection))
            {
                sqlCon.Open();
                SqlDataAdapter sqlDA = new SqlDataAdapter(strSql, sqlCon);
                DataTable dt = new DataTable();
                sqlDA.Fill(dt);
                List<T> config = DTToModel<T>(dt);
                return config;
            }
        }

        /// <summary>
        /// 数据表转换为实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> DTToModel<T>(DataTable dt) where T : class,new()
        {
            Type type = typeof(T);
            PropertyInfo[] files = type.GetProperties();
            List<T> list = new List<T>();
            foreach (DataRow dr in dt.Rows)
            {
                T model = new T();
                for (int i = 0; i < files.Count(); i++)
                {
                    files[i].SetValue(model, dr[i]);
                }
                list.Add(model);
            }
            return list;
        }
    }
}