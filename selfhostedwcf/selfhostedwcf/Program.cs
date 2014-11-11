using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace selfhostedwcf
{
    class Program
    {
        private static ManualResetEvent stop = new ManualResetEvent(false);

        static int Main(string[] args)
        {
            // Hook CTRL-C
            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                stop.Set();
            };

            string strPort = Environment.GetEnvironmentVariable("PORT");
            string host = Environment.GetEnvironmentVariable("VCAP_APP_HOST");
            string applicationInfo = Environment.GetEnvironmentVariable("VCAP_APPLICATION");

            int port = 0;
            dynamic app = null;

            try
            {
                app = JObject.Parse(applicationInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not parse the 'VCAP_APPLICATION' environment variable: {0}", ex.ToString());
                return 1;
            }

            if (!Int32.TryParse(strPort, out port))
            {
                Console.WriteLine("Could not parse the 'PORT' environment variable.");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(host))
            {
                Console.WriteLine("Could not find a 'VCAP_APP_HOST' environment variable.");
                return 1;
            }

            Uri baseAddress = new Uri(string.Format("http://{0}:{1}/", host, port));
     
            BasicHttpBinding binding = new BasicHttpBinding();

            // *******IMPORTANT
            binding.HostNameComparisonMode = HostNameComparisonMode.WeakWildcard;

            Console.WriteLine("Hosting service on address '{0}'", baseAddress);

            using (ServiceHost serviceHost = new ServiceHost(typeof(MyMathServiceLib.MyMathService), baseAddress))
            {
                serviceHost.Description.Behaviors.Add(new ServiceMetadataBehavior 
                { 
                    HttpGetEnabled = true 
                });
                serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>().IncludeExceptionDetailInFaults = true;
                serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>().HttpHelpPageUrl = baseAddress;

                serviceHost.AddServiceEndpoint(typeof(MyMathServiceLib.IMyMathService), binding, baseAddress);

                serviceHost.Open();

                Console.WriteLine("The service is ready at '{0}'", baseAddress);
                Console.WriteLine("Press <CTRL-C> to stop the service.");

                stop.WaitOne();
                Console.WriteLine("<CTRL-C> pressed. Closing service ...");
                // Close the ServiceHost.
                serviceHost.Close();
            }

            return 0;
        }
    }
}

namespace MyMathServiceLib      
{       
    [ServiceContract]       
    public interface IMyMathService       
    {       
        [OperationContract]       
        double Add(double dblNum1, double dblNum2);[OperationContract]      
        double Subtract(double dblNum1, double dblNum2);[OperationContract]      
        double Multiply(double dblNum1, double dblNum2);[OperationContract]      
        double Divide(double dblNum1, double dblNum2);       
    }       

    public class MyMathService : IMyMathService       
    {       
        public double Add(double dblNum1, double dblNum2)       
        {       
            return (dblNum1 + dblNum2);       
        }
        
        public double Subtract(double dblNum1, double dblNum2)      
        {       
            return (dblNum1 - dblNum2);       
        }
        
        public double Multiply(double dblNum1, double dblNum2)      
        {       
            return (dblNum1 * dblNum2);       
        }
        
        public double Divide(double dblNum1, double dblNum2)      
        {       
            return ((dblNum2 == 0) ? 0 : (dblNum1 / dblNum2));       
        }       
    }       
}