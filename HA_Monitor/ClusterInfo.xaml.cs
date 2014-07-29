using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HA_Monitor
{
	/// <summary>
	/// Interaction logic for HAProxyInfo.xaml
	/// </summary>
    public partial class ClusterInfo : UserControl
	{
        public int clusterIndex;

        public ClusterInfo()
		{
			this.InitializeComponent();
		}

        public void setData(ClusterStatusData IData) {
            if (IData.Cluster_Status.CompareTo("Working") == 0) {
                TxtClusterNo.Text = IData.Cluster_Index.ToString();
                TxtCpuUseage.Text = IData.CPU_Useage + " %";
                TxtMemUseage.Text = IData.Available_Memory + " MB";
                TxtTraficTotal.Text = string.Format("{0:F2}", IData.Trafic_Total) + " MB";
                TxtTraficSent.Text = string.Format("{0:F2}", IData.Trafic_Sent) + " MB";
                TxtTraficReceived.Text = string.Format("{0:F2}", IData.Trafic_Received) + " MB";

                if (((SolidColorBrush)StatMarker.Fill).Color != Colors.Green) {
                    StatMarker.Fill = new SolidColorBrush(Colors.Green);
                }
            } else if (IData.Cluster_Status.CompareTo("Updating") == 0) {
                TxtClusterNo.Text = IData.Cluster_Index.ToString();
                TxtCpuUseage.Text = " - ";
                TxtMemUseage.Text = " - ";
                TxtTraficTotal.Text = " - ";
                TxtTraficSent.Text = " - ";
                TxtTraficReceived.Text = " - ";

                if (((SolidColorBrush)StatMarker.Fill).Color != Colors.Yellow) {
                    StatMarker.Fill = new SolidColorBrush(Colors.Yellow);
                }
            } else if (IData.Cluster_Status.CompareTo("Disabled") == 0) {
                TxtClusterNo.Text = IData.Cluster_Index.ToString();
                TxtCpuUseage.Text = " - ";
                TxtMemUseage.Text = " - ";
                TxtTraficTotal.Text = " - ";
                TxtTraficSent.Text = " - ";
                TxtTraficReceived.Text = " - ";

                if (((SolidColorBrush)StatMarker.Fill).Color != Colors.Red) {
                    StatMarker.Fill = new SolidColorBrush(Colors.Red);
                }
            }
        }

        public void setDiable() {
            ((SolidColorBrush)StatMarker.Fill).Color = Colors.Red;
        }
	}
}