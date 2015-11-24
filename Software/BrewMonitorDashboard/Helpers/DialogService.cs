using System.Windows;

namespace BrewMonitorDashboard.Helpers
{
    public class DialogService : IDialogService
    {
        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }
    }
}
