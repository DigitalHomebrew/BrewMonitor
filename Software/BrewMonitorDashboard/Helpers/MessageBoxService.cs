using System.Windows;

namespace BrewMonitorDashboard.Helpers
{
    class MessageBoxService : IMessageBoxService
    {
        public bool ShowMessage(string text, string caption)
        {
            var result = MessageBox.Show(text, caption, MessageBoxButton.OKCancel);
            return result == MessageBoxResult.OK;
        }
    }
}
