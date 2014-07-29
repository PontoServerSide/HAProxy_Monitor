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

    public class ClusteringInfo {
        public string gift;
        public string host;
    }

    public class AvgHost:IComparable<AvgHost> {
        public int avg{get;set;}
        public string IPAddr{get;set;}
        public int Compare(AvgHost avgHost) {
            if (this.avg > avgHost.avg)
                return -1;
            else if (this.avg < avgHost.avg)
                return 1;
            else
                return 0;
        }
    }

    public partial class MainWindow : Window {

        private static Socket server;
        private List<Socket> clientList;
        private List<ClusteringInfo> clusteringInfo;

        private List<HAProxyInfo> haproxyList;
        private List<AvgHost> avgSorted;

        private static byte[] getByte;
        private static byte[] setByte;

        private const int socketPort = 5000;

        public MainWindow() {
            server = new Socket(
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

            clusteringInfo = new List<ClusteringInfo>();
            haproxyList = new List<HAProxyInfo>();

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
                byte[] szData = e.Buffer;    // 데이터 수신k
                string sData = Encoding.UTF8.GetString(szData);

                StatusData ReceivedData = JsonConvert.DeserializeObject<StatusData>(sData);
                Console.WriteLine(ReceivedData.CPU_Useage);
                Console.WriteLine(ReceivedData.Available_Memory);
                Console.WriteLine(ReceivedData.Trafic_Total);
                Console.WriteLine(ReceivedData.Trafic_Sent);
                Console.WriteLine(ReceivedData.Trafic_Received);
                //Console.WriteLine(sData + "\n\n");

                UpdateUI(ReceivedData);

                byte[] temp = Encoding.UTF8.GetBytes("update");
                ClientSocket.Send(temp, temp.Length, SocketFlags.None);
                e.SetBuffer(new byte[4096], 0, 4096);
                ClientSocket.ReceiveAsync(e);
            } else {
                string clientIP = ClientSocket.RemoteEndPoint.ToString();

                ClientSocket.Disconnect(false);
                ClientSocket.Dispose();
                clientList.Remove(ClientSocket);
            }
        }
        #endregion

        #region UpdateUI
        private void UpdateUI (StatusData IData) {
            int index = FindHAProxyClusterEle(IData.HAProxy_IP);
            Dispatcher.BeginInvoke((Action)delegate() {
                if (index != -1) {
                    haproxyList[index].SetData(IData);
                } else {
                    TabMainContent.Items.Add(new TabItem());
                    ((TabItem)TabMainContent.Items[TabMainContent.Items.Count - 1]).Header = IData.HAProxy_IP;

                    haproxyList.Add(new HAProxyInfo());

                    ((TabItem)TabMainContent.Items[TabMainContent.Items.Count - 1]).Content = haproxyList[haproxyList.Count - 1];
                    haproxyList[haproxyList.Count - 1].SetData(IData);
                }
            });

            DynamicClustering();
        }

        private int FindHAProxyClusterEle(string IPAddr) {
            for (int i = 0; i < haproxyList.Count; i++) {
                if (haproxyList[i].HAProxy_IP.CompareTo(IPAddr) == 0) {
                    return i;
                }
            }
            return -1;
        }
        #endregion

        #region DynamicClustering
        private void DynamicClustering() {
            double temp;

            for (int i = 0; i < TabMainContent.Items.Count; i++) {
                temp = ((HAProxyInfo)TabMainContent.Items[i]).AvgCPUUseage();
                avgSorted.Add(new AvgHost { avg = (int)temp, IPAddr = ((HAProxyInfo)TabMainContent.Items[i]).HAProxy_IP });
            }

            avgSorted.Sort();

            for (int j = 0; j < avgSorted.Count; j++) {
                if (avgSorted[j].avg > 90) {
                    for (int i = avgSorted.Count - 1; i >= j; i--) {
                        if (i == j) {
                            j = avgSorted.Count;
                            break;
                        }

                        if (avgSorted[i].avg < 70 && !isHost(avgSorted[i].IPAddr)) {
                            //Attach
                            for (int k = 0; k < clientList.Count; k++) {
                                if (avgSorted[j].IPAddr.CompareTo(clientList[k].RemoteEndPoint.ToString()) == 0) {
                                    byte[] bufferTemp = Encoding.UTF8.GetBytes("A" + avgSorted[i].IPAddr);
                                    clientList[k].Send(bufferTemp, bufferTemp.Length, SocketFlags.None);
                                    clusteringInfo.Add(new ClusteringInfo { host = avgSorted[j].IPAddr, gift = avgSorted[i].IPAddr });
                                    break;
                                }
                            }
                        }
                    }
                }
                if (avgSorted[j].avg > 70) {
                    for (int i = avgSorted.Count - 1; i >= j; i--) {
                        if (i == j) {
                            j = avgSorted.Count;
                            break;
                        }

                        if (avgSorted[i].avg < 60 && !isHost(avgSorted[i].IPAddr) && !isGift(avgSorted[i].IPAddr)) {
                            //Attach
                            for (int k = 0; k < clientList.Count; k++) {
                                if (avgSorted[j].IPAddr.CompareTo(clientList[k].RemoteEndPoint.ToString()) == 0) {
                                    byte[] bufferTemp = Encoding.UTF8.GetBytes("A" + avgSorted[i].IPAddr);
                                    clientList[k].Send(bufferTemp, bufferTemp.Length, SocketFlags.None);
                                    clusteringInfo.Add(new ClusteringInfo { host = avgSorted[j].IPAddr, gift = avgSorted[i].IPAddr });
                                    break;
                                }
                            }
                        }
                    }
                } else {
                    if (isHost(avgSorted[j].IPAddr)) {
                        if (avgSorted[j].avg <= 60) {
                            //Detach
                            for (int k = 0; k < clientList.Count; k++) {
                                if (avgSorted[j].IPAddr.CompareTo(clientList[k].RemoteEndPoint.ToString()) == 0) {
                                    int index = findIndexWithHost(avgSorted[j].IPAddr);

                                    byte[] bufferTemp = Encoding.UTF8.GetBytes("D" + clusteringInfo[index].gift);
                                    clientList[k].Send(bufferTemp, bufferTemp.Length, SocketFlags.None);

                                    clusteringInfo.RemoveAt(index);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool isHost(string IPAddr) {
            for (int i = 0; i < clusteringInfo.Count; i++) {
                if (clusteringInfo[i].host.CompareTo(IPAddr) == 0) {
                    return true;
                }
            }
            return false;
        }

        private bool isGift(string IPAddr) {
            for (int i = 0; i < clusteringInfo.Count; i++) {
                if (clusteringInfo[i].gift.CompareTo(IPAddr) == 0) {
                    return true;
                }
            }
            return false;
        }

        private int findIndexWithHost(string ihost) {
            for (int i = 0; i < clusteringInfo.Count; i++) {
                if (clusteringInfo[i].host.CompareTo(ihost) == 0) {
                    return i;
                }
            }
            return -1;
        }

        private int findClusterInfo(string ihost, string igift) {
            for (int i = 0; i < clusteringInfo.Count; i++) {
                if (clusteringInfo[i].host.CompareTo(ihost) == 0 && clusteringInfo[i].gift.CompareTo(igift) == 0) {
                    return i;
                }
            }
            return -1;
        }
        #endregion
    }
}
