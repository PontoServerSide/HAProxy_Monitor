using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HA_Monitor {
    public class ClusterStatusData {
        public int Cluster_Index { get; set; }
        public int CPU_Useage { get; set; }
        public int Available_Memory { get; set; }
        public Double Trafic_Total { get; set; }
        public Double Trafic_Sent { get; set; }
        public Double Trafic_Received { get; set; }
        public string Cluster_Status { get; set; }
    }
}
