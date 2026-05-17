using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FuzzySharp;
using FuzzySharp.SimilarityRatio;
using FuzzySharp.SimilarityRatio.Scorer.Composite;
using FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;
using Hardware.Info;
using Newtonsoft.Json;
using SteamReqDesktop.Models;
using SteamReqDesktop.Services;
using SteamReqDesktopWPF.models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace SteamReqDesktop.ViewModels {
    internal partial class MainViewModel : ObservableObject {
        // services
        private readonly HardwareInfoService _hardwareInfoService;
        private readonly BenchmarkScoreService _benchmarkScoreService;
        private readonly SteamScraper _scraperService;

        // data 
        [ObservableProperty]
        private string _steamUrlInput = "";

        [ObservableProperty]
        private string _statusMessage = "Ready to scan hardware.";

        [ObservableProperty]
        private HardwareSpec _userHardware;

        [ObservableProperty]
        private GameRequirement _gameRequirements;
        [ObservableProperty]
        private string _cpu_result;
        [ObservableProperty]
        private string _gpu_result;
        [ObservableProperty]
        private string _ram_result;
        [ObservableProperty]
        private bool _isScanComplete = false; // Starts false when the app opens
        [ObservableProperty]
        private string _gameImageUrl;
        [ObservableProperty]
        private bool _isShowingSearchResults = false;
        public ObservableCollection<SteamSearchResults> SearchResults { get; set; } = new ObservableCollection<SteamSearchResults>();


        public ObservableCollection<Storage> Drive_List { get; set; } = new ObservableCollection<Storage>();
        public ObservableCollection<string> Drives_with_space { get; set; } = new ObservableCollection<string>();
        // constructor
        public MainViewModel(HardwareInfoService hardware, BenchmarkScoreService benchmark, SteamScraper scraper) {
            _hardwareInfoService = hardware;
            _benchmarkScoreService = benchmark;
            _scraperService = scraper;

            // get user hardware as app opens since it takes a second
            UserHardware = _hardwareInfoService.GetUserHardware();

        }
        // commands
        [RelayCommand]
        private async Task RunScanAsync() {
            IsScanComplete = false;
            IsShowingSearchResults = false;
            GameRequirements = new GameRequirement();
            if (SteamUrlInput.Contains("store.steampowered.com/app/")) {
                _gameRequirements.Game_Name = "Loading...";
                StatusMessage = "Calling Steam API...";
                try {
                    // Debug.WriteLine("test1");
                    await _scraperService.ScrapeSteamStats(SteamUrlInput, GameRequirements);
                    StatusMessage = "Analyzing Hardware Requirements...";
                    var Scores = _benchmarkScoreService.GetBenchmark();
                    _benchmarkScoreService.SetScores(Scores, UserHardware);
                    List<String> CPUbestMatches = [];
                    foreach (var cpu in _gameRequirements.min_cpu) {
                        var bestMatch = FuzzySharp.Process.ExtractOne(cpu, Scores.CPU_Benchmarks.Keys, scorer: ScorerCache.Get<TokenSetScorer>());
                        if (bestMatch.Score > 95) {
                            string exactCsvKey = bestMatch.Value;
                            CPUbestMatches.Add(exactCsvKey);
                            double benchmarkScore = Scores.CPU_Benchmarks[exactCsvKey];
                        }
                        if(bestMatch.Score <= 95) {

                        }

                    }
                    foreach (var cpu_score in CPUbestMatches) {
                        //Debug.WriteLine(cpu_score);
                        //Console.WriteLine($"STEAM REC CPU {cpu_score} : {Scores.CPU_Benchmarks[cpu_score]}");
                        if (cpu_score.Contains("amd") || cpu_score.Contains("ryzen")) {
                            _gameRequirements.Min_cpu_brand_score_dict.TryAdd("AMD", (cpu_score, Scores.CPU_Benchmarks[cpu_score]));
                        }
                        else if (cpu_score.Contains("intel") || cpu_score.Contains("arc")) {
                            _gameRequirements.Min_cpu_brand_score_dict.TryAdd("Intel", (cpu_score, Scores.CPU_Benchmarks[cpu_score]));
                        }
                        else {
                            _gameRequirements.Min_cpu_brand_score_dict.TryAdd("Other", (cpu_score, Scores.CPU_Benchmarks[cpu_score]));
                        }
                    }

                    List<String> GPUBestMatches = [];
                    foreach (var gpu in _gameRequirements.min_gpu) {
                        var bestMatch = FuzzySharp.Process.ExtractOne(gpu, Scores.GPU_Benchmarks.Keys, scorer: ScorerCache.Get<TokenSetScorer>());
                        if (bestMatch.Score > 95) {
                            string exactCsvKey = bestMatch.Value;
                            GPUBestMatches.Add(exactCsvKey);
                            double benchmarkScore = Scores.GPU_Benchmarks[exactCsvKey];
                        }
                        if (bestMatch.Score <= 95) {
                            string cleanedGpu = Regex.Replace(gpu, @"\(.*?\)", "").Trim();
                            string noVramGpu = Regex.Replace(cleanedGpu, @"\b\d+\s*gb\b", "", RegexOptions.IgnoreCase).Trim();
                            bestMatch = FuzzySharp.Process.ExtractOne(noVramGpu, Scores.GPU_Benchmarks.Keys, scorer: ScorerCache.Get<TokenSetScorer>());
                            //Console.WriteLine($" - {gpu.ToLower()}");
                            if (bestMatch.Score > 75) // Safety threshold so it doesn't add total garbage
                            {
                                GPUBestMatches.Add(bestMatch.Value);
                            }
                        }
                    }
                    // Debug.WriteLine("test2");

                    foreach (var gpu_score in GPUBestMatches) {
                        //Console.WriteLine($"STEAM REC GPU {gpu_score} : {Scores.GPU_Benchmarks[gpu_score]}");
                        if (gpu_score.Contains("nvidia")) {
                            _gameRequirements.Min_gpu_brand_score_dict.TryAdd("Nvidia", (gpu_score, Scores.GPU_Benchmarks[gpu_score]));
                        }
                        else if (gpu_score.Contains("amd") || gpu_score.Contains("radeon")) {
                            _gameRequirements.Min_gpu_brand_score_dict.TryAdd("AMD", (gpu_score, Scores.GPU_Benchmarks[gpu_score]));
                        }
                        else if (gpu_score.Contains("intel") || gpu_score.Contains("arc")) {
                            _gameRequirements.Min_gpu_brand_score_dict.TryAdd("Intel", (gpu_score, Scores.GPU_Benchmarks[gpu_score]));
                        }
                        else {
                            _gameRequirements.Min_gpu_brand_score_dict.TryAdd("Other", (gpu_score, Scores.GPU_Benchmarks[gpu_score]));

                        }
                    }

                    CPUbestMatches = [];
                    foreach (var cpu in _gameRequirements.rec_cpu) {
                        var bestMatch = FuzzySharp.Process.ExtractOne(cpu, Scores.CPU_Benchmarks.Keys, scorer: ScorerCache.Get<TokenSetScorer>());
                        if (bestMatch.Score > 95) {
                            string exactCsvKey = bestMatch.Value;

                            CPUbestMatches.Add(exactCsvKey);
                            double benchmarkScore = Scores.CPU_Benchmarks[exactCsvKey];
                        }

                    }
                    foreach (var cpu_score in CPUbestMatches) {
                        //Console.WriteLine($"STEAM REC CPU {cpu_score} : {Scores.CPU_Benchmarks[cpu_score]}");
                        if (cpu_score.Contains("amd") || cpu_score.Contains("ryzen")) {
                            _gameRequirements.Rec_cpu_brand_score_dict.TryAdd("AMD", (cpu_score, Scores.CPU_Benchmarks[cpu_score]));
                        }
                        else if (cpu_score.Contains("intel") || cpu_score.Contains("arc")) {
                            _gameRequirements.Rec_cpu_brand_score_dict.TryAdd("Intel", (cpu_score, Scores.CPU_Benchmarks[cpu_score]));
                        }
                        else {
                            _gameRequirements.Rec_cpu_brand_score_dict.TryAdd("Other", (cpu_score, Scores.CPU_Benchmarks[cpu_score]));
                        }
                    }

                    GPUBestMatches = [];
                    foreach (var gpu in _gameRequirements.rec_gpu) {
                        var bestMatch = FuzzySharp.Process.ExtractOne(gpu, Scores.GPU_Benchmarks.Keys, scorer: ScorerCache.Get<TokenSetScorer>());
                        if (bestMatch.Score > 95) {
                            string exactCsvKey = bestMatch.Value;
                            GPUBestMatches.Add(exactCsvKey);
                            double benchmarkScore = Scores.GPU_Benchmarks[exactCsvKey];
                        }
                        if (bestMatch.Score <= 95) {
                            string cleanedGpu = Regex.Replace(gpu, @"\(.*?\)", "").Trim();
                            string noVramGpu = Regex.Replace(cleanedGpu, @"\b\d+\s*gb\b", "", RegexOptions.IgnoreCase).Trim();
                            bestMatch = FuzzySharp.Process.ExtractOne(noVramGpu, Scores.GPU_Benchmarks.Keys, scorer: ScorerCache.Get<TokenSetScorer>());
                            //Console.WriteLine($" - {gpu.ToLower()}");
                        }
                    }
                    foreach (var gpu_score in GPUBestMatches) {
                        //Console.WriteLine($"STEAM REC GPU {gpu_score} : {Scores.GPU_Benchmarks[gpu_score]}");
                        if (gpu_score.Contains("nvidia")) {
                            _gameRequirements.Rec_gpu_brand_score_dict.TryAdd("Nvidia", (gpu_score, Scores.GPU_Benchmarks[gpu_score]));
                        }
                        if (gpu_score.Contains("amd") || gpu_score.Contains("radeon")) {
                            _gameRequirements.Rec_gpu_brand_score_dict.TryAdd("AMD", (gpu_score, Scores.GPU_Benchmarks[gpu_score]));
                        }
                        else if (gpu_score.Contains("intel") || gpu_score.Contains("arc")) {
                            _gameRequirements.Rec_gpu_brand_score_dict.TryAdd("Intel", (gpu_score, Scores.GPU_Benchmarks[gpu_score]));
                        }
                        else {
                            if (!_gameRequirements.Rec_gpu_brand_score_dict.ContainsKey("Other")) {
                                continue;
                            }
                            _gameRequirements.Rec_gpu_brand_score_dict.TryAdd("Other", (gpu_score, Scores.GPU_Benchmarks[gpu_score]));

                        }
                    }

                    double minCpuReq = GetRequiredScore(_gameRequirements.Min_cpu_brand_score_dict, UserHardware.CPU_Brand);
                    double recCpuReq = GetRequiredScore(_gameRequirements.Rec_cpu_brand_score_dict, UserHardware.CPU_Brand);

                    if (minCpuReq == 0) {
                        Cpu_result = "No minimum CPU requirements provided by developer. (Possible abnormal formatting on the steam page)";
                    }
                    else if (UserHardware.CPU_Score >= minCpuReq) {
                        if (recCpuReq == 0) {
                            Cpu_result = "Minimum CPU requirement met (No recommended specs provided or abnormal formatting on the steam page)";
                        }
                        else if (UserHardware.CPU_Score >= recCpuReq) {
                            Cpu_result = "Recommended CPU requirement met";
                        }
                        else {
                            Cpu_result = "Minimum CPU requirement met, did not meet recommended CPU requirements";
                        }
                    }
                    else {
                        Cpu_result = "Did not meet Minimum CPU requirements";
                    }

                    double minGpuReq = GetRequiredScore(_gameRequirements.Min_gpu_brand_score_dict, UserHardware.GPU_Brand);
                    double recGpuReq = GetRequiredScore(_gameRequirements.Rec_gpu_brand_score_dict, UserHardware.GPU_Brand);

                    if (minGpuReq == 0) {
                        Gpu_result = "No minimum GPU requirements provided by developer. (Possible abnormal formatting on the steam page)";
                    }
                    else if (UserHardware.GPU_Score >= minGpuReq) {
                        if (recGpuReq == 0) {
                            Gpu_result = "Minimum GPU requirement met (No recommended specs provided or abnormal formatting on the steam page)";
                        }
                        else if (UserHardware.GPU_Score >= recGpuReq) {
                            Gpu_result = "Recommended GPU requirement met";
                        }
                        else {
                            Gpu_result = "Minimum GPU requirement met, did not meet recommended GPU requirements";
                        }
                    }
                    else {
                        Gpu_result = "Did not meet Minimum GPU requirements";
                    }

                    //Drives_with_space.Add("Hard drives with enough space: ");
                    Drives_with_space.Clear();
                    foreach (var drive in UserHardware.Drive_List) {
                        if (_gameRequirements.DisplayStorage == "Required Storage: Unknown") {
                            Drives_with_space.Add("Required Storage Unknown, unable to determine if you have enough storage space");
                            break;
                        }
                        if (drive.FreeSpace >= _gameRequirements.Space_needed_gb && _gameRequirements.DisplayStorage != "Required Storage: Unknown") {
                            Drives_with_space.Add($"{drive.DriveLetter} ({Math.Round(drive.FreeSpace, 2)} GB free)");
                        }
                    }

                    if (_gameRequirements.Min_ram == 0) {
                        Ram_result = "No RAM requirements provided by developer. (Possible abnormal formatting on the steam page)\"";
                    }
                    else if ((UserHardware.Mem_Size + 2) >= _gameRequirements.Min_ram) {
                        if (_gameRequirements.Rec_ram == 0) {
                            Ram_result = "Minimum RAM requirements met (No recommended specs provided or abnormal formatting on the steam page)";
                        }
                        else if ((UserHardware.Mem_Size + 2) >= _gameRequirements.Rec_ram) {
                            Ram_result = "Recommended RAM requirements met";
                        }
                        else {
                            Ram_result = "Minimum RAM requirements met, did not meet recommended RAM requirements";
                        }
                    }
                    else {
                        Ram_result = "Minimum RAM requirements not met";
                    }



                    GameImageUrl = _gameRequirements.Game_image_url;
                    StatusMessage = "Scan Complete";
                    IsScanComplete = true;
                    //StatusMessage = GameRequirements.Space_needed_gb.ToString();
                }
                catch (Exception ex){
                    StatusMessage = $"ERROR: {ex.Message}";
                    _gameRequirements.Game_Name = "Unknown Game";
                }
            }
            else {
                StatusMessage = "Searching Steam...";
                //_gameRequirements.Game_image_url = "";
                GameImageUrl = "";
                var results = await _scraperService.SearchGamesAsync(SteamUrlInput);
                SearchResults.Clear();
                foreach (var game in results) {
                    SearchResults.Add(game);
                }
                IsShowingSearchResults = true;
                StatusMessage = "Select a game from the list.";
            }
        }

        [RelayCommand]
        private async Task SelectGameAsync(int appID) {
            IsShowingSearchResults = false;
            _gameRequirements.Game_Name = "";
            SteamUrlInput = $"https://store.steampowered.com/app/{appID}/";
            await RunScanAsync();
        }

        private double GetRequiredScore(Dictionary<string, (string, double)> reqDict, string userBrand) {
            if (reqDict.Count == 0) return 0; // No requirements were found, return 0!

            string dictKey = "";
            if (userBrand == "amd") dictKey = "AMD";
            else if (userBrand == "nvidia") dictKey = "Nvidia";
            else if (userBrand == "intel") dictKey = "Intel";
            else dictKey = "Other";

            if (reqDict.TryGetValue(dictKey, out var exactMatch)) {
                return exactMatch.Item2;
            }

            foreach (var entry in reqDict) {
                return entry.Value.Item2;
            }

            return 0;
        }
    }
}
