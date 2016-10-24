using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleEntityFramework.Common.ModelsHelper
{
    /// <summary>
    /// 内部属性实体
    /// </summary>
    public class EntityPropertiesEntity
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否为主键  
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// 是否自增长
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// 是否可以为NULL
        /// </summary>
        public bool IsNullable { get; set; }

    }
}
