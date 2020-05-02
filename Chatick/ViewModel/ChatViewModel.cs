using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Net.PeerToPeer;
using System.ServiceModel;
using System.Net;

namespace Chatick
{
    class ChatViewModel
    {
        private P2PService localService;
        private ServiceHost host;
        private PeerName peerName;
        private PeerNameRegistration peerNameRegistration;
        private string serviceUrl;
        private bool isRegistered = false;
        private ChatView viewDelegate;
        public string username;
        private List<P2PInit> peers = new List<P2PInit>();

        public ChatViewModel(ChatView viewDelegate)
        {
            this.viewDelegate = viewDelegate;
        }
        public void OnViewLoaded()
        {
            Debug.WriteLine("opened");

            string port = new Random().Next(0, 5000).ToString();
            username = $"Пользователь {new Random().Next(1, 400).ToString()}";

            RSAService.GenerateKeys();
            startupConnectionWith(port, username);
            updatePeers();
            viewDelegate.setViewTitle(username);
        }

        public void startupConnectionWith(string port, string username) 
        {
            if (isRegistered)
            {
                stopServiceAndRegistration();
            }

            serviceUrl = string.Format("net.tcp://localhost:{0}/P2PService", port);

            if (serviceUrl == null)
            {
                Application.Current.Shutdown();
            }

            localService = new P2PService(this, username, RSAService.publicKey);
            host = new ServiceHost(localService, new Uri(serviceUrl));
            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            host.AddServiceEndpoint(typeof(IP2PService), binding, serviceUrl);

            try
            {
                host.Open();
                this.isRegistered = true;
            }
            catch (AddressAlreadyInUseException)
            {
                Application.Current.Shutdown();
            }

            peerName = new PeerName("P2P Chatick", PeerNameType.Unsecured);

            peerNameRegistration = new PeerNameRegistration(peerName, int.Parse(port));
            peerNameRegistration.Cloud = Cloud.AllLinkLocal;

            peerNameRegistration.Start();

            
        }
        public void OnViewClosing()
        {
            stopServiceAndRegistration();
        }

        public void stopServiceAndRegistration() 
        {
            sendMessage("Покинул канал.");
            peerNameRegistration.Stop();
            host.Close();
            isRegistered = false;
        }

        public void sendMessage(string message)
        {
            peers.ForEach(delegate(P2PInit peer) {
                try
                {
                    byte[] encrypted = RSAService.EncryptWithKey(Encoding.UTF8.GetBytes(message), peer.PublicKey);
                    peer.ServiceProxy.SendMessage(encrypted, username);
                }
                catch (CommunicationException)
                {

                }
            });
        }

        public void updatePeers()
        {
            PeerNameResolver resolver = new PeerNameResolver();
            resolver.ResolveProgressChanged +=
                new EventHandler<ResolveProgressChangedEventArgs>(resolver_ResolveProgressChanged);
            resolver.ResolveCompleted +=
                new EventHandler<ResolveCompletedEventArgs>(resolver_ResolveCompleted);

            peers.Clear();
            viewDelegate.ClearPeers();

            resolver.ResolveAsync(new PeerName("0.P2P Chatick"), 1);
        }

        void resolver_ResolveCompleted(object sender, ResolveCompletedEventArgs e)
        {
            viewDelegate.updatingPeersFinished();
        }

        void resolver_ResolveProgressChanged(object sender, ResolveProgressChangedEventArgs e)
        {
            PeerNameRecord peer = e.PeerNameRecord;
            List<P2PInit> newPeers = new List<P2PInit>();

            foreach (IPEndPoint ep in peer.EndPointCollection)
            {
                if (ep.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    try
                    {

                        string endpointUrl = string.Format("net.tcp://{0}:{1}/P2PService", ep.Address, ep.Port);
                        NetTcpBinding binding = new NetTcpBinding();
                        binding.Security.Mode = SecurityMode.None;
                        IP2PService serviceProxy = ChannelFactory<IP2PService>.CreateChannel(
                            binding, new EndpointAddress(endpointUrl));

                        P2PInit newPeer = new P2PInit
                        {
                            PeerName = peer.PeerName,
                            ServiceProxy = serviceProxy,
                            DisplayString = serviceProxy.GetName(),
                            ButtonsEnabled = true,
                            PublicKey = serviceProxy.GetPublicKey()
                        };
                        newPeers.Add(newPeer);
                    }
                    catch (EndpointNotFoundException)
                    {

                    }
                }
            }
            peers = newPeers;
            viewDelegate.UpdatePeers(newPeers);
            
        }

        public void DisplayMessage(byte[] message, string from)
        {
            string msg;

            try
            {
                byte[] decrypted = RSAService.Decrypt(message);
                msg = Encoding.UTF8.GetString(decrypted);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                msg = "Сообщение не удалось расшифровать. \nПубличный и приватный ключи не совпадают";
            }
            viewDelegate.AppendMessage(msg, from);
            updatePeers();
        }
    }
}
