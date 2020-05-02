using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Chatick
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class P2PService: IP2PService
	{
        private ChatViewModel appViewModel;
        private string username;

        public P2PService(ChatViewModel appViewModel, string username)
        {
            this.appViewModel = appViewModel;
            this.username = username;
        }

        public string GetName()
        {
            return username;
        }

        public void SendMessage(string message, string from)
        {
            appViewModel.DisplayMessage(message, from);
        }
    }
}
