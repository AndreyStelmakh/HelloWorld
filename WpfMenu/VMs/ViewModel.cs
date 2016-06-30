using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using GUIBase.Core;

namespace WpfMenu.VMs
{
    class ViewModel : Archive.UserControls.Mvvm.Extended.ExtendedNotify
    {
        public ObservableCollection<string> Items
        {
            get
            {
                return _Items;

            }

            set
            {
                _Items = value;

                RaisePropertyChanged(() => Items);

            }

        }

        public object SelectedItem
        {
            get
            {
                return _SelectedItem;

            }

            set
            {
                _SelectedItem = value;

                RaisePropertyChanged(() => SelectedItem);

            }

        }

        internal IStringsProvider StringsProvider
        {
            get
            {
                return _StringsProvider;

            }

            set
            {
                _StringsProvider = value;

                RaiseBeforeCollectionChanging();

                Items.Clear();

                foreach (var item in value.GetStrings())
                {
                    Items.Add(item);

                }

                RaiseAfterCollectionChanging();

            }

        }

        public delegate void CollectionChangedEventHandler();

        /// <summary>
        /// Перед обновлением коллекции сообщим потребителям, чтобы не реагировали на каждую вставку в отдельности
        /// </summary>
        public event CollectionChangedEventHandler BeforeCollectionChanging;
        /// <summary>
        /// После обновленя коллекции сообщим потребителям, что пора обновить внешний вид после изменения коллекции
        /// </summary>
        public event CollectionChangedEventHandler AfterCollectionChanging;

        void RaiseBeforeCollectionChanging()
        {
            BeforeCollectionChanging?.Invoke();

        }

        void RaiseAfterCollectionChanging()
        {
            AfterCollectionChanging?.Invoke();

        }

        public ViewModel()
        {
            ShowCommand = new RelayCommand((x)=>
            {
                var selectedItem = SelectedItem as string;

                if (selectedItem != null)
                {
                    MessageBox.Show(selectedItem);

                }

            }, x => SelectedItem != null);

        }

        public RelayCommand ShowCommand
        {
            get
            {
                return _ShowCommand;

            }

            set
            {
                _ShowCommand = value;

                RaisePropertyChanged(() => ShowCommand);

            }

        }

        public IEnumerable<string> GetStrings()
        {
            return new string[] { "a", "abc", "abcd", "ab", "abcde", "abcdef" };

        }

        RelayCommand _ShowCommand;
        private IStringsProvider _StringsProvider;
        private ObservableCollection<string> _Items = new ObservableCollection<string>();
        private object _SelectedItem;

    }

}
