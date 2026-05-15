using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamReqDesktop.Models {
    internal class HardwareSpec {
        public string CPU_print_name {
            get;
            set {  field = value; }
        } = "Unknown CPU";
        public string CPU_Name {
            get;
            set { field = value.ToLower(); } //Normalize to Lower
        } = "";
        public string CPU_Brand {
            get;
            set { field = value.ToLower(); } //Normalize to Lower
        } = "";
        public string GPU_print_name {
            get;
            set { field = value; }
        } = "Unknown GPU";
        public string GPU_Name {
            get;
            set { field = value.ToLower(); } //NMormalize to Lower
        } = "";
        public string GPU_Brand {
            get;
            set { field = value.ToLower(); } //NMormalize to Lower
        } = "";
        public double CPU_Score { get; set; }
        public double GPU_Score { get; set; }
        public double Mem_Size {
            get;
            set { field += (value / 1073741824.0); } //Convert to GB
        }
        public string OS { get; set { field = value.ToLower(); } } = "";
        public List<Storage> Drive_List { get; set; } = new List<Storage>();

    }
    public class Storage {
        private string drive_letter = "";
        private double free_space = 0;

        public string DriveLetter { get { return drive_letter; } set { drive_letter = value.ToUpper(); } }
        public double FreeSpace { get { return free_space; } set { free_space = value / 1073741824.0; } }

        public Storage(string dl, double fp) {
            DriveLetter = dl;
            FreeSpace = fp;
        }
    }
}
