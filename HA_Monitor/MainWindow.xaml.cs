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

using System.Net;
using System.Net.Sockets;

using Newtonsoft.Json;

namespace HA_Monitor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public class StatusData {
        public Double cpu { get; set; }
        public Double mem { get; set; }
        public Double traficTotal { get; set; }
        public Double traficSent { get; set; }
        public Double traficReceived { get; set; }
    }

    public partial class MainWindow : Window {

        private static Socket server;
        private List<Socket> clientList;

        private List<TabControl> haproxyList;

        private static byte[] getByte;
        private static byte[] setByte;

        private const int socketPort = 5000;

        public MainWindow() {
            server = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp);

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, socketPort);
            server.Bind(serverEndPoint);
            server.Listen(10000);

            clientList = new List<Socket>();

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed
                += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
            server.AcceptAsync(args);

            haproxyList = new List<TabControl>();

            InitializeComponent();
        }

        #region Socket
        private void Accept_Completed(object sender, SocketAsyncEventArgs e) {
            Socket ClientSocket = e.AcceptSocket;
            clientList.Add(ClientSocket);

            if (clientList != null) {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                getByte = new byte[4096];
                args.SetBuffer(getByte, 0, 4096);
                args.UserToken = clientList;
                args.Completed
                    += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
                ClientSocket.ReceiveAsync(args);
            }
            e.AcceptSocket = null;
            server.AcceptAsync(e);
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e) {
            Socket ClientSocket = (Socket)sender;
            if (ClientSocket.Connected && e.BytesTransferred > 0) {
                byte[] szData = e.Buffer;    // 데이터 수신
                string sData = Encoding.UTF8.GetString(szData);

                StatusData ReceivedData = JsonConvert.DeserializeObject<StatusData>(sData);
                Console.WriteLine(ReceivedData.cpu);
                Console.WriteLine(ReceivedData.mem);
                Console.WriteLine(ReceivedData.traficTotal);
                Console.WriteLine(ReceivedData.traficSent);
                Console.WriteLine(ReceivedData.traficReceived);
                //Console.WriteLine(sData + "\n\n");

                e.SetBuffer(new byte[4096], 0, 4096);
                ClientSocket.ReceiveAsync(e);
            } else {
                ClientSocket.Disconnect(false);
                ClientSocket.Dispose();
                clientList.Remove(ClientSocket);
            }
        }
        #endregion

        #region UpdateUI
        private void UpdateUI () {

        }

        private int FindHAProxyClustEle(string IPAddr) {
            for (int i = 0; i < haproxyList.Count; i++) {
                
            }
            return -1;
        }
        #endregion
    }
}
