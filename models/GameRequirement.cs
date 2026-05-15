using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SteamReqDesktop.Models {
    public partial class GameRequirement : ObservableObject {
        [ObservableProperty]
        private string _game_Name = "";
        [ObservableProperty]
        private double _min_ram;

        [ObservableProperty]
        private double _rec_ram;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayStorage))]
       
        private double _space_needed_gb;
        public string DisplayStorage => Space_needed_gb == 0 ? "Required Storage: Unknown" : $"Storage: {Space_needed_gb} GB";
        [ObservableProperty]
        private string _game_image_url;
        public ObservableCollection<string> min_cpu { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> min_gpu { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> rec_cpu { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> rec_gpu { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> min_cpu_names { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> min_gpu_names { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<string> rec_cpu_names { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> rec_gpu_names { get; set; } = new ObservableCollection<string>();

        public Dictionary<string, (string, double)> Rec_gpu_brand_score_dict {
            get;
            set;
        } = new Dictionary<string, (string, double)>();
        public Dictionary<string, (string, double)> Min_gpu_brand_score_dict {
            get;
            set;
        } = new Dictionary<string, (string, double)>();

        public Dictionary<string, (string, double)> Min_cpu_brand_score_dict {
            get;
            set;
        } = new Dictionary<string, (string, double)>();
        public Dictionary<string, (string, double)> Rec_cpu_brand_score_dict {
            get;
            set;
        } = new Dictionary<string, (string, double)>();
    }
}
