using System.Collections.Generic;
using System.Windows;

namespace WpfMenu
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new VMs.ViewModel() { StringsProvider = new SomeStringsProvider() };

        }

    }

    class SomeStringsProvider : IStringsProvider
    {
        public IEnumerable<string> GetStrings()
        {
            return new string[] { "a", "abc", "abcd", "ab", "abcde", "abcdef" };

        }

    }

}
