﻿using System;
using SimpleEntityFramework.IBLL;

//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由T4模板自动生成
//     生成时间 2016-10-20 11:30:31 Modify by Ocean
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------
namespace SimpleEntityFramework.BLL
{
	/// <summary>
    /// 业务层大接口
    /// </summary>
	public partial class BLLSession:IBLLSession
	{
  
		#region 00 - ICompanyBLL接口访问对象

		/// <summary>
		/// ICompanyBLL接口访问对象
		/// </summary>	
		private ICompanyBLL _companyBLL;

		/// <summary>
		/// ICompanyBLL接口访问对象
		/// </summary>		
		public ICompanyBLL CompanyBLL
		{
			get{ return _companyBLL ?? new CompanyBLL();}
			set{ _companyBLL = value;}
		}
	
		#endregion
  
		#region 01 - IUserBLL接口访问对象

		/// <summary>
		/// IUserBLL接口访问对象
		/// </summary>	
		private IUserBLL _userBLL;

		/// <summary>
		/// IUserBLL接口访问对象
		/// </summary>		
		public IUserBLL UserBLL
		{
			get{ return _userBLL ?? new UserBLL();}
			set{ _userBLL = value;}
		}
	
		#endregion
	}
}