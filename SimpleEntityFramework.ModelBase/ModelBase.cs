using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEntityFramework.ModelBase
{

    /// <summary>
    /// 数据库表映射基类
    /// </summary>
    public abstract class ModelBase
    {
        /// <summary>
        /// ID属性
        /// </summary>
        public int Id { get; set; }
    }
}
