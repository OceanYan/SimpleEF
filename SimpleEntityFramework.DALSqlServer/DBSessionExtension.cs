﻿using System;
using SimpleEntityFramework.IDAL;

//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由T4模板自动生成
//     生成时间 2016-10-17 21:51:36 Modify by Ocean
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------
namespace SimpleEntityFramework.DALSqlServer
{
	/// <summary>
    /// 数据层大接口，包含所有数据层接口访问方式
    /// </summary>
	public partial class DBSession:IDBSession
	{
  
		#region 00 - ICompanyDal接口访问对象

		/// <summary>
		/// ICompanyDal接口访问对象
		/// </summary>	
		private ICompanyDal _companyDal;

		/// <summary>
		/// ICompanyDal接口访问对象
		/// </summary>		
		public ICompanyDal CompanyDal
		{
			get{ return _companyDal ?? new CompanyDal();}
			set{ _companyDal = value;}
		}
	
		#endregion
  
		#region 01 - IUserDal接口访问对象

		/// <summary>
		/// IUserDal接口访问对象
		/// </summary>	
		private IUserDal _userDal;

		/// <summary>
		/// IUserDal接口访问对象
		/// </summary>		
		public IUserDal UserDal
		{
			get{ return _userDal ?? new UserDal();}
			set{ _userDal = value;}
		}
	
		#endregion
	}
}