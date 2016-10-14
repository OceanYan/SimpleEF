using SimpleEntityFramework.Models;

namespace SimpleEntityFramework.IDAL
{
    //TODO:用T4模板生成

    #region 1.0 User的扩展Dal接口

    /// <summary>
    ///     User的扩展Dal接口
    /// </summary>
    public interface IUserDal : IBaseDal<User>
    {
    }

    #endregion

    #region 2.0 Company的扩展Dal接口

    /// <summary>
    ///     Company的扩展Dal接口
    /// </summary>
    public interface ICompanyDal : IBaseDal<Company>
    {
        
    }

    #endregion
}