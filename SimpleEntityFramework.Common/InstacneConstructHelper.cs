using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEntityFramework.Common
{
    /// <summary>
    /// 对象构造帮助类
    /// </summary>
    public static class InstacneConstructHelper
    {
        /// <summary>
        /// 创建泛型类型对象
        /// </summary>
        /// <param name="assName">程序集名称</param>
        /// <param name="className">类型名称</param>
        /// <param name="genericTypes">泛型类型数组</param>
        /// <returns></returns>
        public static object CreateGenericClassInstance(string assName,string className,Type[] genericTypes)
        {
            Assembly instanceAss = Assembly.Load(assName);
            if (instanceAss == null)
                throw new Exception(string.Format("类型{0}.{1}不存在", assName, className));
            Type instanceType = instanceAss.GetType(assName + "."+ className + "`" + genericTypes.Length);
            if (instanceType == null)
                throw new Exception(string.Format("类型{0}.{1}不存在",assName,className));
            instanceType = instanceType.MakeGenericType(genericTypes);
            return Activator.CreateInstance(instanceType);
        }

        /// <summary>
        ///   创建类型对象
        /// </summary>
        /// <param name="assName">程序集名称</param>
        /// <param name="className">类型名称</param>
        /// <returns></returns>
        public static object CreateInstance(string assName, string className)
        {
            var instanceAss = Assembly.Load(assName);
            if (instanceAss == null)
                throw new Exception(string.Format("类型{0}.{1}不存在", assName, className));
            var instanceType = instanceAss.GetType(string.Format("{0}.{1}", assName, className));
            if (instanceType == null)
                throw new Exception(string.Format("类型{0}.{1}不存在", assName, className));
            return Activator.CreateInstance(instanceType);
        }
    }
}
