using Newtonsoft.Json.Linq;
using SteamReqDesktop.Models;
using SteamReqDesktopWPF.models;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SteamReqDesktop.Services {
    internal class SteamScraper {
        public async Task ScrapeSteamStats(string url, GameRequirement gameReqObj) {
            url = "https://store.steampowered.com/api/appdetails?appids=" + URLParser(url);
            //Console.WriteLine(url);
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept-Language", "en");

            try {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseBody);
                var firstKey = json.Properties().First().Name;
                var pcReqs = json[firstKey]?["data"]?["pc_requirements"];
                string minimum_reqs = (string)pcReqs?["minimum"];
                string recommended_reqs = (string)pcReqs?["recommended"];
                gameReqObj.Game_Name = (string)json[firstKey]?["data"]?["name"];
                string min_text = StripHtml(minimum_reqs);
                string recommended_text = StripHtml(recommended_reqs);
                gameReqObj.Game_image_url = json[firstKey]?["data"]?["header_image"]?.ToString() ?? "";

                //Console.WriteLine(min_text);
                //Console.WriteLine("\n\n");
                //Console.WriteLine(recommended_text);
                //Debug.WriteLine("test");
                if (!string.IsNullOrEmpty(recommended_text)) {

                    var recCpuMatch = Regex.Match(recommended_text, @"Processor:\s*(.+?)\s*Memory:");
                    if (recCpuMatch.Success) {
                        string rawCpu = recCpuMatch.Groups[1].Value;

                        string[] cpuOptions = Regex.Split(rawCpu, @"\s+or\s+|\s*/\s*", RegexOptions.IgnoreCase);

                        //Console.WriteLine("CPU Options:");
                        for (int i = 0; i < cpuOptions.Length; i++) {
                            string cpu = cpuOptions[i];
                            cpu = Regex.Replace(cpu, @"ryzen\s+r(\d)", "Ryzen $1", RegexOptions.IgnoreCase); //find ryzen, search space after, then r followed by a digit, replace with Ryzen and the digit
                            cpu = Regex.Replace(cpu, @"\br(\d)\b", "Ryzen $1", RegexOptions.IgnoreCase); //find r followed by a digit, replace with Ryzen and the digit, but only if it's a standalone word (to avoid replacing things like "Core i7")
                            cpu = Regex.Replace(cpu, @"intel\s+core", "Intel", RegexOptions.IgnoreCase); //find intel core, replace with Intel (to simplify)
                            cpu = Regex.Replace(cpu, @"intel", "Intel Core", RegexOptions.IgnoreCase); //find intel, replace with Intel Core (to simplify)
                            cpu = Regex.Replace(cpu, @"\bi(\d)\s+", "i$1-", RegexOptions.IgnoreCase); //find i followed by a digit and a space, replace with i, the digit, and a dash (to standardize format like "i7-8700K")

                            cpuOptions[i] = cpu.Trim();
                        }

                        foreach (var cpu in cpuOptions) {
                            gameReqObj.rec_cpu.Add(cpu.ToLower() + " ");
                            gameReqObj.rec_cpu_names.Add(cpu + " ");
                        }
                    }
                    var recGpuMatch = Regex.Match(recommended_text, @"Graphics:\s*(.+?)\s*DirectX:");
                    if (recGpuMatch.Success) {
                        string rawGpu = recGpuMatch.Groups[1].Value;
                        string[] gpuOptions = Regex.Split(rawGpu, @"\s+or\s+|\s*/\s*", RegexOptions.IgnoreCase);
                        //Console.WriteLine("GPU Options:");
                        foreach (var gpu in gpuOptions) {
                            //string cleanedGpu = Regex.Replace(gpu, @"\(.*?\)", "");
                            //cleanedGpu = Regex.Replace(cleanedGpu, @"\b\d+\s*gb\b", "", RegexOptions.IgnoreCase);
                            //cleanedGpu = cleanedGpu.Trim();
                            //Console.WriteLine($" - {gpu.ToLower()}");
                            gameReqObj.rec_gpu.Add(gpu.ToLower() + " ");
                            gameReqObj.rec_gpu_names.Add(gpu + " ");
                        }
                    }
                    else {
                        recGpuMatch = Regex.Match(recommended_text, @"Graphics:\s*(.+?)\s*Storage:");
                        if (recGpuMatch.Success) {
                            string rawGpu = recGpuMatch.Groups[1].Value;
                            string[] gpuOptions = Regex.Split(rawGpu, @"\s+or\s+|\s*/\s*", RegexOptions.IgnoreCase);
                            //Console.WriteLine("GPU Options:");
                            foreach (var gpu in gpuOptions) {
                                //string cleanedGpu = Regex.Replace(gpu, @"\(.*?\)", "");
                                //cleanedGpu = Regex.Replace(cleanedGpu, @"\b\d+\s*gb\b", "", RegexOptions.IgnoreCase);
                                //cleanedGpu = cleanedGpu.Trim();
                                //Console.WriteLine($" - {gpu.ToLower()}");
                                gameReqObj.rec_gpu.Add(gpu.ToLower() + " ");
                            }
                        }
                    }
                    var recRamMatch = Regex.Match(recommended_text, @"Memory:\s*(.+?)\s*Graphics:");
                    if (recRamMatch.Success) {
                        string rawRam = recRamMatch.Groups[1].Value;
                        int ram_int = Int32.Parse((Regex.Match(rawRam, @"\d+").Value));
                        //Console.WriteLine($"RAM: {ram_int}");
                        gameReqObj.Rec_ram = ram_int;
                    }

                    var recStorageMatch = Regex.Match(recommended_text, @"Storage:\s*(.+?)\s*");
                    if (recStorageMatch.Success) {
                        string rawStorage = recStorageMatch.Groups[1].Value;
                        int storage_int = Int32.Parse((Regex.Match(rawStorage, @"\d+").Value));
                        gameReqObj.Space_needed_gb = storage_int;
                    }
                }
                //Debug.WriteLine("test2");
                if (!string.IsNullOrEmpty(min_text)) {


                    var minCpuMatch = Regex.Match(min_text, @"Processor:\s*(.+?)\s*Memory:");
                    if (minCpuMatch.Success) {
                        string rawCpu = minCpuMatch.Groups[1].Value;

                        string[] cpuOptions = Regex.Split(rawCpu, @"\s+or\s+|\s*/\s*", RegexOptions.IgnoreCase);

                        //Console.WriteLine("CPU Options:");
                        for (int i = 0; i < cpuOptions.Length; i++) {
                            string cpu = cpuOptions[i];
                            cpu = Regex.Replace(cpu, @"[®™]", "");
                            cpu = Regex.Replace(cpu, @"ryzen\s+r(\d)", "Ryzen $1", RegexOptions.IgnoreCase); //find ryzen, search space after, then r followed by a digit, replace with Ryzen and the digit
                            cpu = Regex.Replace(cpu, @"\br(\d)\b", "Ryzen $1", RegexOptions.IgnoreCase); //find r followed by a digit, replace with Ryzen and the digit, but only if it's a standalone word (to avoid replacing things like "Core i7")
                            cpu = Regex.Replace(cpu, @"intel\s+core", "Intel", RegexOptions.IgnoreCase); //find intel core, replace with Intel (to simplify)
                            cpu = Regex.Replace(cpu, @"intel", "Intel Core", RegexOptions.IgnoreCase); //find intel, replace with Intel Core (to simplify)
                            cpu = Regex.Replace(cpu, @"\bi(\d)\s+", "i$1-", RegexOptions.IgnoreCase); //find i followed by a digit and a space, replace with i, the digit, and a dash (to standardize format like "i7-8700K")
                            cpu = Regex.Replace(cpu, @"\s*@?\s*[\d\.]+\s*(?:GHz|MHz)", "", RegexOptions.IgnoreCase);
                            cpuOptions[i] = cpu.Trim();
                        }

                        foreach (var cpu in cpuOptions) {
                            gameReqObj.min_cpu.Add(cpu.ToLower() + " ");
                            gameReqObj.min_cpu_names.Add(cpu + " ");
                        }
                    }
                    var minGpuMatch = Regex.Match(min_text, @"Graphics:\s*(.+?)\s*DirectX:");
                    if (minGpuMatch.Success) {
                        string rawGpu = minGpuMatch.Groups[1].Value;
                        string[] gpuOptions = Regex.Split(rawGpu, @"\s+or\s+|\s*/\s*", RegexOptions.IgnoreCase);
                        //Console.WriteLine("GPU Options:");
                        foreach (var gpu in gpuOptions) {
                            //string cleanedGpu = Regex.Replace(gpu, @"\(.*?\)", "");
                            //cleanedGpu = Regex.Replace(cleanedGpu, @"\b\d+\s*gb\b", "", RegexOptions.IgnoreCase);
                            //cleanedGpu = cleanedGpu.Trim();
                            //Console.WriteLine($" - {gpu.ToLower()}");
                            gameReqObj.min_gpu.Add(gpu.ToLower() + " ");
                            gameReqObj.min_gpu_names.Add(gpu + " ");
                        }
                    }
                    else {
                        minGpuMatch = Regex.Match(min_text, @"Graphics:\s*(.+?)\s*Storage:");
                        if (minGpuMatch.Success) {
                            string rawGpu = minGpuMatch.Groups[1].Value;
                            string[] gpuOptions = Regex.Split(rawGpu, @"\s+or\s+|\s*/\s*", RegexOptions.IgnoreCase);
                            //Console.WriteLine("GPU Options:");
                            foreach (var gpu in gpuOptions) {
                                //string cleanedGpu = Regex.Replace(gpu, @"\(.*?\)", "");
                                //cleanedGpu = Regex.Replace(cleanedGpu, @"\b\d+\s*gb\b", "", RegexOptions.IgnoreCase);
                                //cleanedGpu = cleanedGpu.Trim();
                                //Console.WriteLine($" - {gpu.ToLower()}");
                                gameReqObj.min_gpu.Add(gpu.ToLower() + " ");
                            }
                        }
                    }
                    var minRamMatch = Regex.Match(min_text, @"Memory:\s*(.+?)\s*Graphics:");
                    if (minRamMatch.Success) {
                        string rawRam = minRamMatch.Groups[1].Value;
                        int ram_int = Int32.Parse((Regex.Match(rawRam, @"\d+").Value));
                        //Console.WriteLine($"RAM: {ram_int}");
                        gameReqObj.Min_ram = ram_int;
                    }

                    var minStorageMatch = Regex.Match(min_text, @"Storage:\s*(\d+)");
                    if (minStorageMatch.Success) {
                        string rawStorage = minStorageMatch.Groups[1].Value;
                        int storage_int = Int32.Parse((Regex.Match(rawStorage, @"\d+").Value));
                        gameReqObj.Space_needed_gb = storage_int;
                        //Console.WriteLine(rawStorage);
                    }
                }

            }
            catch (HttpRequestException e) {
         
                Console.WriteLine($"Request error: {e.Message}");
            }
        }

        public static string StripHtml(string input) {
            if (string.IsNullOrEmpty(input)) {
                return input;
            }
            string textWithoutTags = Regex.Replace(input, "<[^>]*?>", " ");
            string decodeText = WebUtility.HtmlDecode(textWithoutTags);
            return Regex.Replace(decodeText, @"\s+", " ").Trim();
        }

        public string URLParser(string url) {
            string toBeSearched = "https://store.steampowered.com/app/";
            // https://store.steampowered.com/app/1086940/Baldurs_Gate_3/
            int startIndex = url.IndexOf("https://store.steampowered.com/app/");
            if (startIndex == -1) {
                return "Link Cant Be Parsed";
            }
            int ix = url.IndexOf(toBeSearched);
            startIndex += toBeSearched.Length;
            int endIndex = url.IndexOf("/", startIndex);
            if (endIndex == -1) {
                return "Link Cant Be Parsed 2";
            }
            int length = endIndex - startIndex;
            return url.Substring(startIndex, length);

        }

        public async Task<List<SteamSearchResults>> SearchGamesAsync(string searchTerm) {

            string searchUrl = $"https://store.steampowered.com/api/storesearch/?term={searchTerm}&l=english&cc=US";
            using (HttpClient client = new HttpClient()) {
                string jsonResponse = await client.GetStringAsync(searchUrl);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                SteamSearchResponse responseData = JsonSerializer.Deserialize<SteamSearchResponse>(jsonResponse, options);
                if (responseData == null || responseData.Items == null) {
                    return new List<SteamSearchResults>();
                }
                return responseData.Items;
            }

        }
    }
}
