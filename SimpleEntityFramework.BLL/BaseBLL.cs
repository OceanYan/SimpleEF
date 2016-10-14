using System;
using System.Collections.Generic;
using SimpleEntityFramework.Common;
using SimpleEntityFramework.IDAL;

namespace SimpleEntityFramework.BLL
{
    public class BaseBll<T> where T : ModelBase.ModelBase, new()
    {
        private IBaseDal<T> baseDal;


        public IBaseDal<T> BaseDal
        {
            get
            {
                if (baseDal == null)
                {
                    var configValue = Common.ConfigHelper.GetConfig("DalType");
                    if (string.IsNullOrEmpty(configValue))
                    {
                        throw new Exception("配置：DalType 不存在");
                    }
                    var assInfo = configValue.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    baseDal = Common.InstacneConstructHelper.CreateGenericClassInstance(assInfo[0],
                        assInfo[1], new Type[] {typeof (T)}) as IBaseDal<T>;
                }
                return baseDal;
            }
        }

        public T GetById(int id)
        {
            return BaseDal.GetById(id);
        }

        public List<T> GetAll()
        {
            return baseDal.GetAll();
        } 
    }
}