using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Collections.Generic;
using System.Globalization;
namespace SteamReqDesktop.Models {
    internal class BenchmarkScores {
        public Dictionary<string, double> CPU_Benchmarks { get; private set; } = new Dictionary<string, double>();

        public Dictionary<string, double> GPU_Benchmarks { get; private set; } = new Dictionary<string, double>();

        public void LoadCPU(Dictionary<string, double> data) {
            CPU_Benchmarks = data;
        }

        public void LoadGPU(Dictionary<string, double> data) {
            GPU_Benchmarks = data;
        }
    }
    public class BenchmarkInfo {
        public string DeviceName { get; set; } = "";
        public double MedianScore { get; set; }
        public int NumberOfBenchmarks { get; set; } = 0;
    }



}
    internal class GpuCsvRecord {
    // Tells CsvHelper to look for the column literally named "manufacturer"
    [Name("manufacturer")]
    public string Manufacturer { get; set; } = "";

    [Name("name")]
    public string Name { get; set; } = "";

        [Name("single_float_performance_gflop_s")]
        public double? Gflops { get; set; }
    }
