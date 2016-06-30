using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using RegimST.VMs;

namespace RegimST
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContext = new vmMain();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as vmMain;

            if (vm != null)
            {
                if (vm.AddItemCommand.CanExecute(null))
                {
                    (DataContext as vmMain).AddItemCommand.Execute(null);

                }

            }

        }

        private void SomeControl_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            var control = sender as System.Windows.Controls.Control;

            if (control != null)
            {
                e.ManipulationContainer = WorkCanvas;

            }

        }

        private void SomeControl_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            var control = sender as System.Windows.Controls.Control;

            if (control != null)
            {
                var vm = DataContext as vmMain;

                if (vm != null)
                {
                    var m = new vmMain.ManipulationArgs(control.DataContext as vmMain.ObjectPositionDescriptor, e);

                    if (vm.ManipulationCommand.CanExecute(m))
                    {
                        vm.ManipulationCommand.Execute(m);

                    }

                    e.Handled = true;

                }

            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();

        }

    }

    /// <summary>
    /// Конвертер выдирающий компоненту угла поворота из матрицы
    /// </summary>
    class MatrixToRotation : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Media.Matrix)
            {
                //todo:
                return 5;

            }
            else
            {
                return value;

            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();

        }

    }

    /// <summary>
    /// Конвертер выдирающий компоненту увеличения из матрицы
    /// </summary>
    class MatrixToScale : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Media.Matrix)
            {
                //todo:
                return 5;

            }
            else
            {
                return value;

            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();

        }

    }

    class LeftComponentMatrixConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is System.Windows.Media.Matrix)
            {
                return ((System.Windows.Media.Matrix)value).OffsetX;

            }
            else
            {
                return null;

            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class RightComponentMatrixConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Media.Matrix)
            {
                return ((System.Windows.Media.Matrix)value).OffsetY;

            }
            else
            {
                return null;

            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();

        }

    }

}
