using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hardware.Info;
using SteamReqDesktop.Models;
namespace SteamReqDesktop.Services {
    internal class HardwareInfoService {
        public HardwareSpec GetUserHardware() {
            var User_Hardware = new HardwareSpec();
            IHardwareInfo hardwareInfo = new HardwareInfo();
            try {
                hardwareInfo.RefreshOperatingSystem();
                hardwareInfo.RefreshCPUList();
                hardwareInfo.RefreshDriveList();
                hardwareInfo.RefreshMemoryList();
                hardwareInfo.RefreshVideoControllerList();
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            User_Hardware.OS = hardwareInfo.OperatingSystem.Name.Trim();
            User_Hardware.CPU_Name = hardwareInfo.CpuList[0].Name.Trim();
            User_Hardware.CPU_print_name = hardwareInfo.CpuList[0].Name.Trim();
            if (User_Hardware.CPU_Name.Contains("amd") || User_Hardware.CPU_Name.Contains("ryzen")) {
                User_Hardware.CPU_Brand = "amd";
            }else if (User_Hardware.CPU_Name.Contains("intel")) {
                User_Hardware.CPU_Brand = "intel";
            }
            else {
                User_Hardware.CPU_Brand = "other";
            }
            User_Hardware.GPU_Name = hardwareInfo.VideoControllerList[0].Name.Trim();
            User_Hardware.GPU_print_name = hardwareInfo.VideoControllerList[0].Name.Trim();
            if (User_Hardware.GPU_Name.Contains("nvidia")) {
                User_Hardware.GPU_Brand = "nvidia";
            }else if (User_Hardware.GPU_Name.Contains("amd") || User_Hardware.GPU_Name.Contains("radeon")) {
                User_Hardware.GPU_Brand = "amd";
            } else if (User_Hardware.GPU_Name.Contains("arc") || User_Hardware.GPU_Name.Contains("intel")){
                User_Hardware.GPU_Brand = "intel";
            }else {
                User_Hardware.GPU_Brand = "other";

            }
            foreach (var Mem_Stick in hardwareInfo.MemoryList) {
                User_Hardware.Mem_Size = Mem_Stick.Capacity;
            }
            foreach (var Drive in hardwareInfo.DriveList) {
                foreach (var partition in Drive.PartitionList) {
                    foreach (var volume in partition.VolumeList) {
                        User_Hardware.Drive_List.Add(new Storage(volume.Caption, volume.FreeSpace));
                    }
                }
            }

            return User_Hardware;
        }

    }
}
