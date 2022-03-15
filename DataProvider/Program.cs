using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProvider
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();

            #region Get Object
            //Guid id = new Guid("CD340601-1743-49B9-8981-2BCED474C064");
            //string data = client.GetUserNameById(id);
            //Console.WriteLine(data);
            #endregion

            #region Select Row
            //string data = client.SelectUserData();
            //Console.WriteLine(data);
            #endregion

            #region Save
            //string name = "Muhammad Khoirudin";
            //string address = "Jogjakarta, Indonesia";

            //string data = client.SaveUser(new Guid("CD340601-1743-49B9-8981-2BCED474C064"), name, address);
            //Console.WriteLine(data);
            #endregion

            #region Save With Return Output Parameter
            //string name = "Naskala";
            //string address = "Sleman, Indonesia";

            //string data = client.InsertUser(name, address);
            //Console.WriteLine(data);
            #endregion

            #region Return Data Table
            string data = client.SelectUserInfo();
            Console.WriteLine(data);
            #endregion

            Console.ReadKey();
        }
    }
}
