using System;
using System.Configuration;
using System.Reflection;

namespace InvoiceService.Common
{
    public class Util
    {
        public string GetServiceName()
        {
            string serviceName = string.Empty;

            try
            {
                Assembly executingAssembly = Assembly.GetAssembly(typeof(ProjectInstaller));
                string targetDir = executingAssembly.Location;
                Configuration config = ConfigurationManager.OpenExeConfiguration(targetDir);
                serviceName = config.AppSettings.Settings["ServiceName"].Value.ToString();

                return serviceName;
            }
            catch (Exception ex)
            {
                return "VNSInvoiceService";
            }
        }
    }
}
