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

        private ChatViewModel viewModel;

        public ChatView()
        {
            InitializeComponent();
            viewModel = new ChatViewModel(this);
        }

        private void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            viewModel.OnViewLoaded();
        }
        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            viewModel.OnViewClosing();
        }


        public void UpdatePeers(List<P2PInit> peers)
        {
            ClearPeers();
            peers.ForEach(delegate (P2PInit peer)
                {
                    PeerList.Items.Add(peer);
                }
            );
        }

        private void SendMessagePressed(object sender, RoutedEventArgs e)
        {
            string messageToSend = MessageText.Text;

            if (messageToSend == "") { return; }
            MessageText.Text = "";
            viewModel.sendMessage(messageToSend);
            pushMessage(new ChatMessage()
            {
                MessageText = messageToSend,
                MessageAuthor = "Я"
            }
            );
        }
        private void pushMessage(ChatMessage message)
        {
            MessagesList.Items.Add(message);
            MessagesList.SelectedIndex = MessagesList.Items.Count - 1;
            MessagesList.ScrollIntoView(MessagesList.SelectedItem);
            MessagesList.SelectedIndex = -1;
        }
        private void UpdatePeersButtonPressed(object sender, RoutedEventArgs e)
        {
            viewModel.updatePeers();
        }

        public void ClearPeers()
        {
            PeerList.Items.Clear();
        }
        public void AppendMessage(string message, string from)
        {
            pushMessage(new ChatMessage()
            {
                MessageText = message,
                MessageAuthor = from
            }
            );
        }
        public void updatingPeersFinished()
        {
            if (PeerList.Items.Count == 0)
            {
                PeerList.Items.Add(
                   new P2PInit
                   {
                       DisplayString = "К сожалению, участников нет...",
                       ButtonsEnabled = true
                   });
            }
        }

        public void setViewTitle(string title)
        {
            Title = title;
        }

    }
}
