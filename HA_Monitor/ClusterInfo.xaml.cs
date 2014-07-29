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
        private int clusterIndex { get; set; }

        public ClusterInfo()
		{
			this.InitializeComponent();
		}

        public void setData(double cpu, double mem, double traficTotal, double traficSent, double traficRecieve, string status) {
            if (status.CompareTo("Working") == 0) {
                TxtCpuUseage.Text = Math.Round(cpu, 2).ToString() + " %";
                TxtMemUseage.Text = Math.Round(mem, 2).ToString() + " MB";
                TxtTraficTotal.Text = Math.Round(traficTotal, 0).ToString() + " MB";
                TxtTraficSent.Text = Math.Round(traficSent, 0).ToString() + " MB";
                TxtTraficReceived.Text = Math.Round(traficRecieve, 0).ToString() + " MB";

                if (((SolidColorBrush)StatMarker.Fill).Color != Colors.Green) {
                    ((SolidColorBrush)StatMarker.Fill).Color = Colors.Green;
                }
            } else if (status.CompareTo("Updating") == 0) {
                TxtCpuUseage.Text = " - ";
                TxtMemUseage.Text = " - ";
                TxtTraficTotal.Text = " - ";
                TxtTraficSent.Text = " - ";
                TxtTraficReceived.Text = " - ";

                if (((SolidColorBrush)StatMarker.Fill).Color != Colors.Yellow) {
                    ((SolidColorBrush)StatMarker.Fill).Color = Colors.Yellow;
                }
            } else if (status.CompareTo("Disabled") == 0) {
                TxtCpuUseage.Text = " - ";
                TxtMemUseage.Text = " - ";
                TxtTraficTotal.Text = " - ";
                TxtTraficSent.Text = " - ";
                TxtTraficReceived.Text = " - ";

                if (((SolidColorBrush)StatMarker.Fill).Color != Colors.Red) {
                    ((SolidColorBrush)StatMarker.Fill).Color = Colors.Red;
                }
            }
        }
	}
}