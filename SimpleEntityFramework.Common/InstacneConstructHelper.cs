using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEntityFramework.Common
{
    public static class InstacneConstructHelper
    {
        public static object CreateGenericClassInstance(string assName,string className,Type[] genericTypes)
        {
            Assembly ass = Assembly.Load(assName);
            Type instanceType = ass.GetType(assName + "."+ className + "`" + genericTypes.Length);
            if (instanceType == null)
            {
                throw new Exception(string.Format("类型{0}.{1}不存在",assName,className));
            }
            instanceType = instanceType.MakeGenericType(genericTypes);
            return Activator.CreateInstance(instanceType);
        }
    }
}
