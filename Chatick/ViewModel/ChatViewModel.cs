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
    class ChatViewModel
    {
        private P2PService localService;
        private ServiceHost host;
        private PeerName peerName;
        private PeerNameRegistration peerNameRegistration;
        private string serviceUrl;
        private bool isRegistered = false;
        public void OnViewLoaded()
        {
            Debug.WriteLine("opened");

            string port = new Random().Next(0, 5000).ToString();
            string username = $"Пользователь {new Random().Next(1, 400).ToString()}";

            startupConnectionWith(port, username);
        }

        public void startupConnectionWith(string port, string username) 
        {
            if (isRegistered)
            {
                stopServiceAndRegistration();
            }
            // Установка заголовка окна
            //this.Title = string.Format("P2P приложение - {0}", port);

            serviceUrl = string.Format("net.tcp://localhost:{0}/P2PService", port);

            //this.Title = serviceUrl;
            // Выполнение проверки, не является ли адрес null
            if (serviceUrl == null)
            {
                // Отображение ошибки и завершение работы приложения
                //MessageBox.Show(this, "Не удается определить адрес конечной точки WCF.", "Networking Error",
                //MessageBoxButton.OK, MessageBoxImage.Stop);
                Application.Current.Shutdown();
            }

            // Регистрация и запуск службы WCF
            localService = new P2PService(this, username);
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
                // Отображение ошибки и завершение работы приложения
                //MessageBox.Show(this, "Не удается начать прослушивание, порт занят.", "WCF Error",
                //MessageBoxButton.OK, MessageBoxImage.Stop);
                Application.Current.Shutdown();
            }

            // Создание имени равноправного участника (пира)
            peerName = new PeerName("P2P Sample", PeerNameType.Unsecured);

            // Подготовка процесса регистрации имени равноправного участника в локальном облаке
            peerNameRegistration = new PeerNameRegistration(peerName, int.Parse(port));
            peerNameRegistration.Cloud = Cloud.AllLinkLocal;

            // Запуск процесса регистрации
            peerNameRegistration.Start();
        }
        public void onViewClosing()
        {
            stopServiceAndRegistration();
        }

        public void stopServiceAndRegistration() 
        {
            // Stop peer registration
            peerNameRegistration.Stop();

            // Stop WCF-service
            host.Close();

            isRegistered = false;
        }

            public void sendMessage(string message, RoutedEventArgs e)
        {
            if (((Button)e.OriginalSource).Name == "MessageButton")
            {
                // Получение пира и прокси, для отправки сообщения
                P2PInit peerEntry = ((Button)e.OriginalSource).DataContext as P2PInit;
                if (peerEntry != null && peerEntry.ServiceProxy != null)
                {
                    try
                    {
                        peerEntry.ServiceProxy.SendMessage(message, ConfigurationManager.AppSettings["username"]);
                    }
                    catch (CommunicationException)
                    {

                    }
                }
            }
        }

        public void updatePeers()
        {
            Debug.WriteLine("Clicked update");

            // Создание распознавателя и добавление обработчиков событий
            PeerNameResolver resolver = new PeerNameResolver();
            resolver.ResolveProgressChanged +=
                new EventHandler<ResolveProgressChangedEventArgs>(resolver_ResolveProgressChanged);
            resolver.ResolveCompleted +=
                new EventHandler<ResolveCompletedEventArgs>(resolver_ResolveCompleted);

            // Подготовка к добавлению новых пиров
            //PeerList.Items.Clear();
            //RefreshButton.IsEnabled = false;

            // Преобразование незащищенных имен пиров асинхронным образом
            resolver.ResolveAsync(new PeerName("0.P2P Sample"), 1);
        }

        void resolver_ResolveCompleted(object sender, ResolveCompletedEventArgs e)
        {
            /*
            // Сообщение об ошибке, если в облаке не найдены пиры
            if (PeerList.Items.Count == 0)
            {
                PeerList.Items.Add(
                   new P2PInit
                   {
                       DisplayString = "Пиры не найдены.",
                       ButtonsEnabled = true
                   });
            }
            // Повторно включаем кнопку "обновить"
            RefreshButton.IsEnabled = true;
            */
        }

        void resolver_ResolveProgressChanged(object sender, ResolveProgressChangedEventArgs e)
        {
            PeerNameRecord peer = e.PeerNameRecord;

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

                        /*
                        PeerList.Items.Add(
                           new P2PInit
                           {
                               PeerName = peer.PeerName,
                               ServiceProxy = serviceProxy,
                               DisplayString = serviceProxy.GetName(),
                               ButtonsEnabled = true
                           });
                        */
                    }
                    catch (EndpointNotFoundException ex)
                    {

                    }
                }
            }
        }

        public void peerListItemPressed(RoutedEventArgs e)
        {
            Debug.WriteLine("Peer list pressed");

            // Убедимся, что пользователь щелкнул по кнопке с именем MessageButton
            if (((Button)e.OriginalSource).Name == "MessageButton")
            {
                // Получение пира и прокси, для отправки сообщения
                P2PInit peerEntry = ((Button)e.OriginalSource).DataContext as P2PInit;
                if (peerEntry != null && peerEntry.ServiceProxy != null)
                {
                    try
                    {
                        Debug.WriteLine("Peer list pressed");
                        peerEntry.ServiceProxy.SendMessage("Привет друг!", ConfigurationManager.AppSettings["username"]);
                        Debug.WriteLine("Peer list pressed");
                    }
                    catch (CommunicationException)
                    {

                    }
                }
            }
        }

        public void DisplayMessage(string message, string from)
        {
            Debug.WriteLine(message);
            //MessageBox.Show(this, message, string.Format("Сообщение от {0}", from),
            //MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
