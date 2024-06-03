using CS_NeuralNetwork;
using CS_NeuralNetwork_WPF.ModelView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CS_NeuralNetwork_WPF.View
{
    /// <summary>
    /// Логика взаимодействия для NetVeier.xaml
    /// </summary>
    public partial class NetVier : UserControl
    {
        private Dictionary<Brush,EllipseGeometry> ellipses;
        private Dictionary<Pen, LineGeometry> lines;
        internal ViewModel vm;

        private const int ellipse_radius = 7;
        private const int line_width = 1;
        private const int distance_between_ellipses = 30;
        private const int distance_between_layers = 120;

        public NetVier()
        {
            InitializeComponent();
            ellipses = new Dictionary<Brush,EllipseGeometry>();
            lines = new Dictionary<Pen, LineGeometry>();
        }
        internal void Update()
        {
            if (vm.net == null)
                return;

            ellipses.Clear();
            lines.Clear();


                int layer = 1;
                int biggest_layer_width = vm.net.Layers.Max(L => L.COUNT); 
                var ellipses_alpha = new List<double>();
                var lines_alpha = new List<List<double>>();var r = new Random();

                foreach (var L in vm.net.Layers)
                {
                    
                    if(L is Layer_IN)
                    {
                        for (int i = 0; i < L.COUNT; i++)
                            ellipses_alpha.Add(0.7);
                    }
                    else
                        ellipses_alpha = CS_NeuralNetwork.DataFormat.Scaling(L.Biases.ToArray());
                        

                    for (int q = 0; q < L.COUNT; q++)
                    {
                        var pn = (byte)(ellipses_alpha[q] * 255);
                        ellipses.Add(new SolidColorBrush(Color.FromArgb(pn, 255, 200, 81)),
                                     new EllipseGeometry(new Point(distance_between_layers * layer,
                                                                   distance_between_ellipses * (q + (biggest_layer_width - L.COUNT) / 2.0)),
                                                                   ellipse_radius, ellipse_radius)
                        );


                    lines_alpha = CS_NeuralNetwork.DataFormat.Scaling(L.Weights);
                        for (int e = 0; e < L.Weights[q].Count; e++)
                        {
                        
                            var br = (byte)(lines_alpha[q][e] * 255);
                            br = br < 128 ? (byte)0 : br;
                            lines.Add(new Pen(new SolidColorBrush(Color.FromArgb(br, br, br, br)), line_width),
                                      new LineGeometry(new Point(distance_between_layers * layer, distance_between_ellipses * (q + (biggest_layer_width - L.COUNT) / 2.0)),
                                                       new Point(distance_between_layers * (layer - 1), distance_between_ellipses * (e + (biggest_layer_width - vm.net.Layers[layer - 2].COUNT) / 2.0))
                            ));
                        }
                    }
                    biggest_layer_width = biggest_layer_width < L.COUNT ? L.COUNT : biggest_layer_width;
                    layer++;
                }
            
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            foreach (var geometry in lines)
                drawingContext.DrawGeometry (null, geometry.Key, geometry.Value);
            foreach (var geometry in ellipses)
                drawingContext.DrawGeometry(geometry.Key, new Pen(new SolidColorBrush(Color.FromRgb(30,30,30)), 1), geometry.Value);   
            
        }
    }
}
