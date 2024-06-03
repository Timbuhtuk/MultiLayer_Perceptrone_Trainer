using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CS_NeuralNetwork;
using CS_NeuralNetwork_WPF.View;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace CS_NeuralNetwork_WPF.ModelView
{
    internal class NetModel : INotifyPropertyChanged
    {
        public NetModel(ref Config config)
        {
            this.Config = config;
        }
        public Config Config { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Net_save { 
            get { return Config.net_save; }
            set
            {
                try {
                    using (StreamReader reader = new StreamReader(value))
                    {
                        var row = reader.ReadToEnd();
                        List<NetSave> Saves = JsonConvert.DeserializeObject<List<NetSave>>(row);
                        Inputs = Saves[0].Weights.Count.ToString();
                        var HL = new StringBuilder();
                        for(int q = 1; q < Saves.Count - 1; q++)
                        {
                            HL.Append(Saves[q].Biases.Count);
                            if(q!= Saves.Count - 2)
                            {
                                HL.Append(' ');
                            }
                        }
                        Hidden_Layers = HL.ToString();
                        Outputs = Saves.Last().Biases.Count.ToString();
                    }
                } 
                catch { }
                Config.net_save = value; NotifyProperyChanged();
            }
        }
        public string Inputs { get { return Config.inputs.ToString(); } set { Config.inputs = Int32.Parse(value); NotifyProperyChanged(); } }
        public string Outputs { get { return Config.outputs.ToString(); } set { Config.outputs = Int32.Parse(value); NotifyProperyChanged(); } }

        public string Hidden_Layers
        {
            get
            {
                var res = new StringBuilder();
                foreach (var q in Config.hiddenlayers)
                {
                    res.Append(q.ToString());
                    res.Append(" ");
                }
                return res.ToString();
            }
            set
            {
                var res_str = value.Split(" ");
                int[] res = new int[res_str.Length];
                for (int q = 0; q < res_str.Length; q++)
                {
                    Int32.TryParse(res_str[q],out res[q]);
                }
                Config.hiddenlayers = res;
                NotifyProperyChanged();
            }
        }

        private ICommand? net_save_file_brows;
        public ICommand? Net_save_file_brows => net_save_file_brows ??= new BaseCommand(
                param =>
                {
                    Net_save = getFilename(Net_save);
                }
            );
        private string getFilename(string file)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "txt files (*.json)|*.json|All files (*.*)|*.*";
            dialog.ShowDialog();
            if (dialog.FileName != null && dialog.FileName != "")
            {
                return dialog.FileName;
            }
            return file;
        }
        private void NotifyProperyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
