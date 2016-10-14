using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleEntityFramework.IDAL;
using SimpleEntityFramework.Models;

namespace SimpleEntityFramework.DALSqlServer
{
    public class UserDal : BaseDal<User>, IUserDal
    {
    }

    public class CompanyDal : BaseDal<Company>, ICompanyDal
    {
    }
}
