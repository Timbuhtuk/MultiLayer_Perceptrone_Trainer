using CS_NeuralNetwork;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CS_NeuralNetwork_WPF.ModelView
{
    internal class LearnModel
    {
        public LearnModel(ref Config config)
        {
            this.Config = config;
        }
        public Config Config { get; set; }
        public Window Owner { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;


        public string Epoch { get { return Config.epoch.ToString(); } set { Int32.TryParse(value,out Config.epoch); NotifyProperyChanged(); } }
        public string LR { get { return Config.lr.ToString(); } set { Double.TryParse(value, out Config.lr); NotifyProperyChanged(); } }
        public string AC { get { return Config.acelleration.ToString(); } set { Double.TryParse(value, out Config.acelleration); NotifyProperyChanged(); } }
        public string Threads { get { return Config.threads.ToString(); } set { Int32.TryParse(value, out Config.threads); NotifyProperyChanged(); } }
        public bool Use_Threads { get { return Config.multithread; } set { Config.multithread = value; NotifyProperyChanged(); } }
        public bool Stochastic { get { return Config.stochastic; } set { Config.stochastic = value; NotifyProperyChanged(); } }
        public string Stop_At { get { return Config.stop_learn.ToString(); } set { Double.TryParse(value, out Config.stop_learn); NotifyProperyChanged(); } }
       
        private void NotifyProperyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
