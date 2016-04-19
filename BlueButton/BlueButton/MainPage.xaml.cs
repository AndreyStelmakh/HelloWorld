using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BlueButton
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
        private ObservableCollection<ButtonContent> _PivotItem1Content = new ObservableCollection<ButtonContent>();
        private ObservableCollection<ButtonContent> _PivotItem2Content = new ObservableCollection<ButtonContent>();
        private ObservableCollection<ButtonContent> _PivotItem3Content = new ObservableCollection<ButtonContent>();

        public MainPage()
        {
            this.InitializeComponent();

            DataContext = this;

            IpAddress = "192.168.1.107";
            Port = 2121;

            PivotItem1Content.Add(new ButtonContent() { Name = "зайчик" });
            PivotItem1Content.Add(new ButtonContent() { Name = "слоник" });
            PivotItem1Content.Add(new ButtonContent() { Name = "рыба" });
            PivotItem2Content.Add(new ButtonContent() { Name = "дерево" });
            PivotItem2Content.Add(new ButtonContent() { Name = "куст" });
            PivotItem2Content.Add(new ButtonContent() { Name = "молния" });
            PivotItem2Content.Add(new ButtonContent() { Name = "небо" });
            PivotItem2Content.Add(new ButtonContent() { Name = "лес" });
            PivotItem2Content.Add(new ButtonContent() { Name = "волны" });
            PivotItem2Content.Add(new ButtonContent() { Name = "огонь" });
            PivotItem2Content.Add(new ButtonContent() { Name = "цветы" });
            PivotItem2Content.Add(new ButtonContent() { Name = "ветер" });
            PivotItem3Content.Add(new ButtonContent() { Name = "circle" });
            PivotItem3Content.Add(new ButtonContent() { Name = "square" });
            PivotItem3Content.Add(new ButtonContent() { Name = "треугольник" });
            PivotItem3Content.Add(new ButtonContent() { Name = "line" });

        }

        public ObservableCollection<ButtonContent> PivotItem1Content
        {
            get
            {
                return _PivotItem1Content;
            }

            set
            {
                _PivotItem1Content = value;
            }
        }

        public ObservableCollection<ButtonContent> PivotItem2Content
        {
            get
            {
                return _PivotItem2Content;
            }

            set
            {
                _PivotItem2Content = value;
            }
        }

        public ObservableCollection<ButtonContent> PivotItem3Content
        {
            get
            {
                return _PivotItem3Content;
            }

            set
            {
                _PivotItem3Content = value;
            }
        }

        public class ButtonContent : INotifyPropertyChanged
        {
            private Windows.UI.Xaml.Media.Imaging.BitmapImage _Image;

            public Windows.UI.Xaml.Media.Imaging.BitmapImage Image
            {
                get
                {
                    return _Image;
                }

                set
                {
                    _Image = value;

                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Image"));

                    }

                }

            }

            public string Name
            {
                get
                {
                    return _Name;

                }

                set
                {
                    _Name = value;

                    if(PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Name"));

                    }

                }

            }

            private string _Name;

            public event PropertyChangedEventHandler PropertyChanged;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = e.OriginalSource as Button;

            if(button != null)
            {
                var dataContext = button.DataContext as ButtonContent;

                if (dataContext != null)
                {
                    //var r = this.Resources["_img1"] as Bitmap;
                    //ImageSource j;
                    //var y = new Windows.UI.Xaml.Media.Imaging.WriteableBitmap(5, 8) ;//..SetSource()
                    //y.SetSourceAsync()
                    //y.
                    //foreach(var pixel in y.PixelBuffer.ToArray())
                    //{


                    //}
                    ////var str = dataContext.Image.ToString();
                    //var str = new Windows.UI.Xaml.Media.Imaging.WriteableBitmap(5, 8).PixelBuffer.ToArray();

                    InsCommand.Execute(
                        new System.Net.IPEndPoint(System.Net.IPAddress.Parse(IpAddress), Port),
                        null);

                }

            }

        }

    }

}
