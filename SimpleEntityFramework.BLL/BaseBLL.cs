using System;
using System.Collections.Generic;
using SimpleEntityFramework.Common;
using SimpleEntityFramework.DALSqlServer;
using SimpleEntityFramework.IBLL;
using SimpleEntityFramework.IDAL;
// ReSharper disable InconsistentNaming

namespace SimpleEntityFramework.BLL
{
    public abstract class BaseBLL<T> : IBaseBLL<T> where T : ModelBase.ModelBase, new()
    {
        protected BaseBLL()
        {
            SetDal();
        }

        protected IDAL.IBaseDal<T> _dal;

        protected abstract void SetDal();

        private IDBSession _dbSession;

        // ReSharper disable once InconsistentNaming
        public IDAL.IDBSession DBSession
        {
            get
            {
                if (null == _dbSession)
                {
                    var dalConfig = Common.ConfigHelper.GetConfig("DALType");
                    if (string.IsNullOrEmpty(dalConfig))
                        throw new Exception("数据层配置不存在！");
                    if (!dalConfig.Contains(","))
                        throw new Exception("数据层配置不合法！");
                    var config = dalConfig.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    var retDbSessionFactory = Common.InstacneConstructHelper.CreateInstance(config[0], config[1]) as IDBSessionFactory;
                    if (null == retDbSessionFactory)
                        throw new Exception("没有找到数据层工厂类型！");
                    _dbSession = retDbSessionFactory.GetDBSession();
                }
                return _dbSession;
            }
        }

        #region 1.0 通过ID查找实体 - T GetById(int id)

        /// <summary>
        ///     通过ID查找实体
        /// </summary>
        /// <param name="id">查询的ID</param>
        /// <returns></returns>
        public T GetById(int id)
        {
            return _dal.GetById(id);
        }

        #endregion

        #region 2.0 获取全部实体 - List<T> GetAll()

        /// <summary>
        ///     获取全部实体
        /// </summary>
        /// <returns></returns>
        public List<T> GetAll()
        {
            return _dal.GetAll();
        }

        #endregion

        #region 3.0 新增实体 - int Add(T model)

        /// <summary>
        ///     新增实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Add(T model)
        {
            return _dal.Add(model);
        }

        #endregion

        #region 4.0 根据id删除 - int Del(T model)

        /// <summary>
        ///     根据id删除
        /// </summary>
        /// <param name="model">包含要删除id的对象</param>
        /// <returns></returns>
        public int Del(T model)
        {
            return _dal.Del(model);
        }

        #endregion

        #region 5.0 根据条件删除 - bool DelBy(Func<T,bool> delWhere)

        /// <summary>
        ///     根据条件删除
        /// </summary>
        /// <param name="delWhere"></param>
        /// <returns></returns>
        public bool DelBy(Func<T, bool> delWhere)
        {
            return _dal.DelBy(delWhere);
        }

        #endregion

        #region 6.0 修改 - int Modify(T model, params string[] proNames)

        /// <summary>
        ///     修改
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="proNames">要修改的属性名称</param>
        /// <returns></returns>
        public int Modify(T model, params string[] proNames)
        {
            return _dal.Modify(model, proNames);
        }

        #endregion

        #region 7.0 批量修改 +int Modify(T model, Func<T,bool> whereLambda, params string[] modifiedProNames)

        /// <summary>
        ///     批量修改
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="whereLambda">查询条件</param>
        /// <param name="modifiedProNames">要修改的属性名称</param>
        /// <returns></returns>
        public int ModifyBy(T model, Func<T, bool> whereLambda, params string[] modifiedProNames)
        {
            return _dal.ModifyBy(model, whereLambda, modifiedProNames);
        }

        #endregion

        #region 8.0 根据条件查询 +List<T> GetListBy(Func<T,bool> whereLambda)

        /// <summary>
        ///     根据条件查询
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public List<T> GetListBy(Func<T, bool> whereLambda)
        {
            return _dal.GetListBy(whereLambda);
        }

        #endregion
    }
}