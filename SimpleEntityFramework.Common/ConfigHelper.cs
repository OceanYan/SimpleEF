using System.Configuration;

namespace SimpleEntityFramework.Common
{
    /// <summary>
    /// 配置帮助类
    /// </summary>
    public static class ConfigHelper
    {
        #region 1.0 获取AppSettings中配置 - string GetConfig(string key)

        /// <summary>
        ///     获取AppSettings中配置
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetConfig(string key)
        {
            //不返回Null，避免空指针异常，调用时判断。
            return ConfigurationManager.AppSettings[key] ?? "";
        }

        #endregion

        #region 2.0 获取ConnectionStrings配置的连接字符串 - string GetConnectionString(string conName)

        /// <summary>
        ///     获取ConnectionStrings配置的连接字符串
        /// </summary>
        /// <param name="conName"></param>
        /// <returns></returns>
        public static string GetConnectionString(string conName)
        {
            var element = ConfigurationManager.ConnectionStrings[conName];
            return element == null ? "" : element.ToString();
        }

        #endregion
    }
}