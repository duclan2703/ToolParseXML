using log4net;
using System;
using System.Configuration;
using System.ServiceProcess;
using System.Timers;

namespace InvoiceService
{
    public partial class ScheduleService : ServiceBase
    {
        private Timer timer = null;
        ILog log = LogManager.GetLogger(typeof(ScheduleService));
        public ScheduleService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer = new Timer();
            double waitTime;
            if (double.TryParse(ConfigurationManager.AppSettings["WaitTime"], out waitTime))
                timer.Interval = waitTime * 15000;
            else
                timer.Interval = 180000;
            // Enable timer
            timer.Enabled = true;
            timer.Elapsed += timer_Tick;
        }
        private void timer_Tick(object sender, ElapsedEventArgs args)
        {
            try
            {
                timer.Stop();
                VNSService srv = new VNSService();
                srv.Processing();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                timer.Start();
            }
        }

        protected override void OnStop()
        {
            timer.Enabled = true;
            timer.Stop();
        }
    }
}
