using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cfexewcftest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ServiceReference1.MyMathServiceClient client = new ServiceReference1.MyMathServiceClient())
            {
                Console.WriteLine(client.Add(2, 3));
            }
        }
    }
}
