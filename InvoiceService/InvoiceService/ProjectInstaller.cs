using InvoiceService.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace InvoiceService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            this.VNSInvoiceService.ServiceName = new Util().GetServiceName();
        }

        private void VNSInvoice_AfterInstall(object sender, InstallEventArgs e)
        {
            using (ServiceController sc = new ServiceController(new Util().GetServiceName()))
            {
                sc.Start();
            }
        }
    }
}
