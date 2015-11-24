using System;
using System.Net;
using BrewMonitor;
using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;

namespace DashboardWPF.ViewModels.Monitor
{
    public sealed class MonitorViewModel: Screen, IMainScreenTabItem
    {
        private readonly IBrewMonitorService _brewMonitorService;
        private readonly DialogCoordinator _dialogCoordinator;

        public MonitorViewModel(IBrewMonitorService brewMonitorService, DialogCoordinator dialogCoordinator)
        {
            _brewMonitorService = brewMonitorService;
            _dialogCoordinator = dialogCoordinator;
            DisplayName = "monitor";
        }

        public void SendData()
        {
            try
            {
                const string writekey = "YOUR_KEY";
                const string strUpdateBase = "http://api.thingspeak.com/update";
                var strUpdateUri = strUpdateBase + "?key=" + writekey;
                const string strField1 = "18";
                const string strField2 = "42";

                strUpdateUri += "&field1=" + strField1;
                strUpdateUri += "&field2=" + strField2;

                var thingsSpeakReq = (HttpWebRequest)WebRequest.Create(strUpdateUri);
                var thingsSpeakResp = (HttpWebResponse)thingsSpeakReq.GetResponse();

                if (string.Equals(thingsSpeakResp.StatusDescription, "OK")) return;
                var exData = new Exception(thingsSpeakResp.StatusDescription);
                throw exData;
            }
            catch (Exception ex)
            {
                //lblError.InnerText = ex.Message;
                //lblError.Style.Add("display", "block");
                //throw;
            }
        }
    }
}
