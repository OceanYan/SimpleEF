using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleEntityFramework.Common.ModelsHelper
{
    /// <summary>
    ///     生成帮助类
    /// </summary>
    public class GenerationHelper
    {
        /// <summary>
        /// 获取实体的类型集合
        /// </summary>
        /// <param name="modelsAssemblyPath"></param>
        /// <param name="superAssemblyPath"></param>
        /// <param name="superName"></param>
        /// <returns></returns>
        public static List<Type> GetModels(string modelsAssemblyPath, string superAssemblyPath, string superName)
        {
            var modelsAss = Assembly.LoadFrom(modelsAssemblyPath);
            var superAss = Assembly.LoadFrom(superAssemblyPath);
            var superType = superAss.GetType(superName);
            return modelsAss.GetTypes().Where(type =>
                type.BaseType != null && type.BaseType == superType
                ).ToList();
        }

        #region 1 实体生成操作

        #region 1.0 获取实体生成数据 -public List<ModelGenerateData> GetModelsGeneratData

        /// <summary>
        ///     获取生成数据
        /// </summary>
        /// <param name="connectionStr">连接字符串</param>
        /// <param name="databaseName">数据库名</param>
        /// <param name="modelsProPath"></param>
        /// <param name="tableNames">表名</param>
        /// <param name="projectPath"></param>
        /// <param name="parentAssemblyName">父类的解决方案名称</param>
        /// <param name="parentName">父类名称</param>
        /// <returns></returns>
        public static List<ModelGenerateData> GetModelsGeneratData(string connectionStr, string databaseName,
            string modelsProPath,
            string tableNames = "", string projectPath = "",
            string parentAssemblyName = "", string parentName = "")
        {
            var ret = new List<ModelGenerateData>();
            //获取父类属性信息 
            var parentPropeties = GetSuperPropertyInfos(projectPath, parentAssemblyName, parentName);
            //获取数据库字段属性
            var dbTables = DbHelper.GetDbTables(connectionStr, databaseName, tableNames);
            //把数据库的属性拼接为GenerateData
            foreach (var tb in dbTables)
            {
                var tbColunms = DbHelper.GetDbColumns(connectionStr, databaseName, tb.TableName);
                ret.Add(PackingSourceToGenerateData(tb.TableName, tbColunms,
                    string.IsNullOrEmpty(parentName) || string.IsNullOrEmpty(parentAssemblyName)
                        ? ""
                        : string.Format("{0}.{1}", parentAssemblyName, parentName), parentPropeties));
            }
            //生成属性配置文件
            //ModelsPropertiesHelper.RecorderModelsProperties(modelsProPath, ret);

            return ret;
        }

        #endregion

        #region 1.1 获取父类属性 - private List<PropertyInfo> GetSuperProperty(Type superType)

        /// <summary>
        ///     获取父类属性
        /// </summary>
        /// <param name="superType"></param>
        /// <returns></returns>
        private static List<PropertyInfo> GetSuperProperty(Type superType)
        {
            if (superType == null)
                return new List<PropertyInfo>();
            return superType.GetProperties().ToList();
        }

        #endregion

        #region 1.2 获取父类属性 - private List<PropertyInfo> GetSuperPropertyInfos(string ,string ,string)

        /// <summary>
        ///     获取父类属性
        /// </summary>
        /// <param name="projectPath">解决方案位置</param>
        /// <param name="parentAssemblyName">父类的解决方案名称</param>
        /// <param name="parentName">父类名称</param>
        /// <returns></returns>
        private static List<PropertyInfo> GetSuperPropertyInfos(string projectPath, string parentAssemblyName,
            string parentName)
        {
            var propertyInfos = new List<PropertyInfo>();
            if (!string.IsNullOrEmpty(projectPath) && !string.IsNullOrEmpty(parentAssemblyName) &&
                !string.IsNullOrEmpty(parentName))
            {
                var superAss = Assembly.LoadFrom(projectPath);
                if (superAss != null)
                {
                    var superType = superAss.GetType(string.Format("{0}.{1}", parentAssemblyName, parentName));
                    propertyInfos = GetSuperProperty(superType);
                }
            }
            return propertyInfos;
        }

        #endregion

        #region 1.3 把原始数据拼装程GeneratData - private ModelGenerateData PackingSourceToGenerateData

        /// <summary>
        ///     把原始数据拼装程GeneratData
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columns">数据列</param>
        /// <param name="parentName">父类名称</param>
        /// <param name="parentPropeties">父类属性</param>
        /// <returns></returns>
        private static ModelGenerateData PackingSourceToGenerateData(string tableName, List<DbColumn> columns,
            string parentName, List<PropertyInfo> parentPropeties)
        {
            var generateData = new ModelGenerateData();
            //先不不判断父类属性是否为空，可能存在父类只为约束使用，没有属性
            if (!string.IsNullOrEmpty(parentName))
            {
                generateData.HasParent = true;
                generateData.ParentName = parentName;
                if (columns != null && columns.Count > 0)
                {
                    var matchCol =
                        columns.FindAll(col => parentPropeties.Exists(pro => string.Equals(pro.Name, col.ColumnName)));
                    //如果父类的属性个数和子类一样，那就判断为可继承
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

        #endregion

        #endregion
    }

    #region 生成类数据实体 - ModelGenerateData

    /// <summary>
    ///     生成类数据实体
    /// </summary>
    public class ModelGenerateData
    {
        /// <summary>
        ///     表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        ///     数据列
        /// </summary>
        public List<DbColumn> TbColumns { get; set; }

        /// <summary>
        ///     是否存在父类
        /// </summary>
        public bool HasParent { get; set; }

        /// <summary>
        ///     父类名称
        /// </summary>
        public string ParentName { get; set; }
    }

    #endregion
}