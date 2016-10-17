using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SimpleEntityFramework.Common.ModelsHelper
{
    /// <summary>
    ///     实体数据库属性帮助类
    /// </summary>
    public class ModelsPropertiesHelper
    {
        /// <summary>
        ///     配置文件名称
        /// </summary>
        private static readonly string PropertiesFileName = "ModelsPropertis.xml";

        /// <summary>
        ///     记录实体的数据库属性
        /// </summary>
        /// <param name="path"></param>
        /// <param name="generateDatas"></param>
        public static void RecorderModelsProperties(string path, List<ModelGenerateData> generateDatas)
        {
            //Path.Combine(path, "ModelsPropertis.xml");
            var document = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            var innerPropertiesElement = new XElement("TableInnerProperties");
            GenerateInnerProperties(innerPropertiesElement, generateDatas);
            document.Add(innerPropertiesElement);
            document.Save(Path.Combine(path, PropertiesFileName));
        }

        /// <summary>
        ///     生成表内部的属性
        /// </summary>
        /// <param name="innerProEle"></param>
        /// <param name="generateDatas"></param>
        private static void GenerateInnerProperties(XElement innerProEle, List<ModelGenerateData> generateDatas)
        {
            generateDatas.ForEach(data =>
            {
                var tablesEle = new XElement("Tables", new XAttribute("Name", data.TableName));
                data.TbColumns.ForEach(col =>
                {
                    tablesEle.Add(new XElement("Columns",
                        new XAttribute("Name", col.ColumnName),
                        new XAttribute("IsPrimaryKey", col.IsPrimaryKey),
                        new XAttribute("IsIdentity", col.IsIdentity),
                        new XAttribute("IsNullable", col.IsNullable)
                        ));
                });
                innerProEle.Add(tablesEle);
            });
        }

        /// <summary>
        ///     获取内部配置实体
        /// </summary>
        /// <param name="configPath"></param>
        /// <param name="tabName"></param>
        /// <returns></returns>
        public static List<InnerPropertyEntity> GetInnerProperties(string configPath, string tabName)
        {
            var filePath = Path.Combine(configPath, PropertiesFileName);

            if (!File.Exists(filePath))
                return new List<InnerPropertyEntity>();

            var document = XDocument.Load(filePath);
            //获取TableInnerProperties接点
            var innerProperitesElement = document.Elements("TableInnerProperties").ToArray();
            if (innerProperitesElement == null)
                throw new Exception("获取实体属性配置项异常，获取：TableInnerProperties配置时获取为NULL。");
            if (innerProperitesElement.Length == 0)
                throw new Exception("获取实体属性配置项异常，表内部属性配置：TableInnerProperties不存在。");
            if (innerProperitesElement.Length > 1)
                throw new Exception("获取实体属性配置项异常，表内部属性配置：TableInnerProperties存在重复配置。");
            //获取表配置接点
            var tabElements =
                innerProperitesElement.Elements("Tables")
                    .Where(element => string.Equals(tabName, GetAttributeValue(element, "Name")))
                    .ToArray();
            if (tabElements == null)
                throw new Exception(string.Format("获取实体属性配置项异常：获取表：{0} 的内部属性配置时获取为NULL。", tabName));
            if (tabElements.Length > 1)
                throw new Exception(string.Format("获取实体属性配置项异常，表：{0} 的内部属性配置存在重复配置。", tabName));

            return tabElements.Length == 0
                ? new List<InnerPropertyEntity>()
                : tabElements[0].Elements("Columns").Select(column => new InnerPropertyEntity
                {
                    ColumnName = GetAttributeValue(column, "Name"),
                    IsPrimaryKey = Convert.ToBoolean(GetAttributeValue(column, "IsPrimaryKey")),
                    IsIdentity = Convert.ToBoolean(GetAttributeValue(column, "IsIdentity")),
                    IsNullable = Convert.ToBoolean(GetAttributeValue(column, "IsNullable"))
                }).ToList();
        }

        /// <summary>
        ///     获取属性的值
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attrName"></param>
        /// <returns></returns>
        private static string GetAttributeValue(XElement element, string attrName)
        {
            return element == null || element.Attribute(attrName) == null
                ? string.Empty
                // ReSharper disable once PossibleNullReferenceException
                : element.Attribute(attrName).Value;
        }
    }
}