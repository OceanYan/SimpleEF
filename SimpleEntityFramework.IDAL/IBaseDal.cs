using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SimpleEntityFramework.IDAL
{
    public interface IBaseDal<T>
        where T : ModelBase.ModelBase
            , new()
    {
        #region 1.0 通过ID查找实体 - T GetById(int id)

        /// <summary>
        ///     通过ID查找实体
        /// </summary>
        /// <param name="id">查询的ID</param>
        /// <returns></returns>
        T GetById(int id);

        #endregion

        #region 2.0 获取全部实体 - List<T> GetAll()

        /// <summary>
        ///     获取全部实体
        /// </summary>
        /// <returns></returns>
        List<T> GetAll();

        #endregion

        #region 3.0 新增实体 - int Add(T model)

        /// <summary>
        ///     新增实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        int Add(T model);

        #endregion

        #region 4.0 根据id删除 - int Del(T model)

        /// <summary>
        ///     根据id删除
        /// </summary>
        /// <param name="model">包含要删除id的对象</param>
        /// <returns></returns>
        int Del(T model);

        #endregion

        #region 5.0 根据条件删除 - bool DelBy(Func<T,bool> delWhere)

        /// <summary>
        ///     根据条件删除
        /// </summary>
        /// <param name="delWhere"></param>
        /// <returns></returns>
        bool DelBy(Func<T,bool> delWhere);

        #endregion

        #region 6.0 修改 - int Modify(T model, params string[] proNames)

        /// <summary>
        ///     修改
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="proNames">要修改的属性名称</param>
        /// <returns></returns>
        int Modify(T model, params string[] proNames);

        #endregion

        #region 7.0 批量修改 +int Modify(T model, Func<T,bool> whereLambda, params string[] modifiedProNames)

        /// <summary>
        ///     批量修改
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="whereLambda">查询条件</param>
        /// <param name="modifiedProNames">要修改的属性名称</param>
        /// <returns></returns>
        int ModifyBy(T model, Func<T, bool> whereLambda, params string[] modifiedProNames);

        #endregion

        #region 8.0 根据条件查询 +List<T> GetListBy(Func<T,bool> whereLambda)

        /// <summary>
        ///     根据条件查询
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        List<T> GetListBy(Func<T, bool> whereLambda);

        #endregion
    }
}