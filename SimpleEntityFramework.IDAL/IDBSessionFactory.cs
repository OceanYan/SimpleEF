using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEntityFramework.IDAL
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// 数据层访问工厂接口
    /// </summary>
    public interface IDBSessionFactory
    {
        /// <summary>
        /// 获取DBSession方法
        /// </summary>
        /// <returns></returns>
        IDBSession GetDBSession();
    }
}
