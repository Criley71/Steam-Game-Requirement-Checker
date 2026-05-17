using CsvHelper;
using CsvHelper.Configuration;
using SteamReqDesktop.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SteamReqDesktop.Services {
    internal class BenchmarkScoreService {
        public BenchmarkScores GetBenchmark() {
            BenchmarkScores benchmark = new BenchmarkScores();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                NewLine = Environment.NewLine,
            };
            string baseFolder = AppContext.BaseDirectory;
            string fullCpuCsvPath = Path.Combine(baseFolder, "data", "Blender_CPU_Benchmarks.csv");
            using (var reader = new StreamReader(fullCpuCsvPath))
            using (var csv = new CsvReader(reader, config)) {
                var records = csv.GetRecords<BenchmarkInfo>();
                Dictionary<string, double> cpu_results = records.GroupBy(r => r.DeviceName.ToLower()).ToDictionary(g => g.Key.ToLower(), g => g.OrderByDescending(x => x.NumberOfBenchmarks).First().MedianScore);
                benchmark.LoadCPU(cpu_results);
            }
            string fullGpuCsvPath = Path.Combine(baseFolder, "data", "dbgpu.csv");
            Dictionary<string, double> gpu_results = LoadGpuBenchmarks(fullGpuCsvPath);
            benchmark.LoadGPU(gpu_results);
// Console.WriteLine(gpu_results["nvidia geforce rtx 5070 ti"]);
            return benchmark;
        }

        public void SetScores(BenchmarkScores benchmarks, HardwareSpec users_hardware) {
            users_hardware.CPU_Score = benchmarks.CPU_Benchmarks[users_hardware.CPU_Name];
            users_hardware.GPU_Score = benchmarks.GPU_Benchmarks[users_hardware.GPU_Name];
        }

        public void PrintUserBenchmark(HardwareSpec users_hardware) {

        }

        public Dictionary<string, double> LoadGpuBenchmarks(string filePath) {

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);


            var records = csv.GetRecords<GpuCsvRecord>().ToList();
            var benchmarkDict = new Dictionary<string, double>(System.StringComparer.OrdinalIgnoreCase);

            foreach (var record in records) {
                if (!record.Gflops.HasValue) continue;


                string combinedKey = $"{record.Manufacturer} {record.Name}".ToLower();


                if (!benchmarkDict.ContainsKey(combinedKey)) {
                    benchmarkDict.Add(combinedKey, record.Gflops.Value);
                }
            }
            return benchmarkDict;
        }
    }
}
