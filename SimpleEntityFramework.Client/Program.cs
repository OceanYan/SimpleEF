using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleEntityFramework.BLL;
using SimpleEntityFramework.Models;

namespace SimpleEntityFramework.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            BaseBll<User> baseBll = new BaseBll<User>();
            baseBll.GetById(1);
            var userList = baseBll.GetAll();
            //1.获取UserType最大的：
            var maxValue = userList.Max(user => user.UserType);
            var maxUser = userList.Where(user => user.UserType == maxValue);
            foreach (User u in maxUser)
            {
                Console.WriteLine("UserType最大的用户为：{0}", u.Name);
            }
            //2.获取最小的
            var sortList = userList.OrderBy(user => user.UserType);
            Console.WriteLine("UserType最小的用户为：{0}",sortList.First().Name);
            //3.平均值
            var totalValue = userList.Aggregate(0.0, (result, user) => result + user.UserType);
            Console.WriteLine("UserType最平均值为：{0}",totalValue/userList.Count);
            //4.除去前三个外的所有元素个数
            int count = userList.Skip(3).Count();
            Console.WriteLine("除去前三个外的所有元素：{0}",count);
            //5.除去前三个外的所有元素个数后再取3个
            var users = userList.Skip(3).Take(3);
            foreach (User u in users)
            {
                Console.WriteLine("你好，我叫{0}",u.Name);
            }
        }
    }
}
