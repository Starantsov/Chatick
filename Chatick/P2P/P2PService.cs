using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Security.Cryptography;

namespace Chatick
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class P2PService : IP2PService
    {
        private ChatViewModel appViewModel;
        private string username;
        private RSAParameters publicKey;

        public P2PService(ChatViewModel appViewModel, string username, RSAParameters publicKey)
        {
            this.appViewModel = appViewModel;
            this.username = username;
            this.publicKey = publicKey;
        }

        public string GetName()
        {
            return username;
        }

        public RSAParameters GetPublicKey()
        {
            return publicKey;
        }

        public void SendMessage(byte[] message, string from)
        {
            appViewModel.DisplayMessage(message, from);
        }
    }
}
