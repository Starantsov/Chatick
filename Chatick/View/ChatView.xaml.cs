using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Net.PeerToPeer;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Net;
using System.Configuration;

namespace Chatick
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class ChatView : Window
	{
		
        private ChatViewModel viewModel = new ChatViewModel();

		public ChatView()
		{
			InitializeComponent();
		}
        
        private void OnWindowLoad(object sender, RoutedEventArgs e)
		{
            viewModel.OnViewLoaded();
        }
		private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            viewModel.onViewClosing();
        }

        private void sendMessage(string message, RoutedEventArgs e)
        {
            viewModel.sendMessage(message, e);
        }

        private void UpdateButtonPressed(object sender, RoutedEventArgs e)
        {
            viewModel.updatePeers();
        }

        private void PeerListPressed(object sender, RoutedEventArgs e)
        {
            viewModel.peerListItemPressed(e);
        }
    }
}
