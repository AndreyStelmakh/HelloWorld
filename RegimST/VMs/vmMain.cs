using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

//todo:
using System.Windows.Media.Imaging;

using Archive.UserControls.Mvvm.Extended;
using Global.GUI.Core;

namespace RegimST.VMs
{
    public class vmMain : ExtendedNotify
    {
        public ObservableCollection<ObjectPositionDescriptor> ObjectPositionDescriptors
        {
            get
            {
                return _objectPositionDescriptors;

            }

            set
            {
                _objectPositionDescriptors = value;

                RaisePropertyChanged(() => ObjectPositionDescriptors);

            }

        }

        /// <summary>
        /// Добавляет объект в рабочую коллекцию где попало
        /// </summary>
        public RelayCommand AddItemCommand
        {
            get
            {
                return _addItemCommand;

            }

            set
            {
                _addItemCommand = value;

                RaisePropertyChanged(() => AddItemCommand);

            }

        }

        /// <summary>
        /// Трансформирует объект. Свойства преобразования передаются в аргументе типа ManipulationArgs команды
        /// </summary>
        public RelayCommand ManipulationCommand
        {
            get
            {
                return _manipulationCommand;

            }

            set
            {
                _manipulationCommand = value;

                RaisePropertyChanged(() => ManipulationCommand);

            }

        }

        Random _random = new Random();

        public vmMain()
        {
            _images = new ObservableCollection<BitmapImage>(new BitmapImage[]{
                            new BitmapImage(new Uri("Images/diagramm.png", UriKind.Relative)),
                            new BitmapImage(new Uri("Images/building.png", UriKind.Relative)),
                            new BitmapImage(new Uri("Images/accept.png", UriKind.Relative)),
                            new BitmapImage(new Uri("Images/santa.png", UriKind.Relative))
                        });

            AddItemCommand = new RelayCommand(
                    (a) => {
                        ObjectPositionDescriptors.Add(new ObjectPositionDescriptor()
                        {
                            Left = 50 + 300*_random.NextDouble(),
                            Top = 50 + 300*_random.NextDouble(),
                            Images = _images
                        });

                    });

            ManipulationCommand = new RelayCommand(
                    (a) => {
                        var d = a as ManipulationArgs;

                        if (d != null)
                        {
                            if (d.DeltaEventArgs != null)
                            {
                                var currentMatrix = d.PositionDescriptor.ManipulationMatrix;

                                currentMatrix.Rotate(d.DeltaEventArgs.DeltaManipulation.Rotation);
                                currentMatrix.Scale(d.DeltaEventArgs.DeltaManipulation.Scale.X, d.DeltaEventArgs.DeltaManipulation.Scale.Y);

                                //System.Windows.MessageBox.Show("");

                                d.PositionDescriptor.ManipulationMatrix = currentMatrix;

                            }
                            //todo: отладка
                            else
                            {
                                var currentMatrix = d.PositionDescriptor.ManipulationMatrix;
                                currentMatrix.Rotate(5);
                                currentMatrix.Scale(1.01, 1.01);

                                d.PositionDescriptor.ManipulationMatrix = currentMatrix;

                            }

                        }

                    });

        }

        private ObservableCollection<ObjectPositionDescriptor> _objectPositionDescriptors = new ObservableCollection<ObjectPositionDescriptor>();
        private ObservableCollection<BitmapImage> _images;

        private RelayCommand _addItemCommand;
        private RelayCommand _manipulationCommand;

        /// <summary>
        /// Тип описателя объекта (только необходимые свойства)
        /// </summary>
        public class ObjectPositionDescriptor : ExtendedNotify
        {
            public double Left
            {
                get
                {
                    return _left;

                }

                set
                {
                    _left = value;

                    RaisePropertyChanged(() => _left);

                }

            }

            public double Top
            {
                get
                {
                    return _top;

                }

                set
                {
                    _top = value;

                    RaisePropertyChanged(() => _top);

                }

            }

            /// <summary>
            /// Коллекция кадров для демонстрации
            /// </summary>
            public ObservableCollection<BitmapImage> Images
            {
                get
                {
                    return _images;

                }

                set
                {
                    _images = value;

                    RaisePropertyChanged(() => Images);

                }

            }

            public Matrix ManipulationMatrix
            {
                get
                {
                    return _manipulationMatrix;

                }

                set
                {
                    _manipulationMatrix = value;

                    RaisePropertyChanged(() => ManipulationMatrix);

                }

            }

            private double _left;
            private double _top;
            private Matrix _manipulationMatrix;

            private ObservableCollection<BitmapImage> _images;

        }

        public class ManipulationArgs
        {
            public ManipulationArgs(ObjectPositionDescriptor positionDescriptor, System.Windows.Input.ManipulationDeltaEventArgs deltaEventArgs)
            {
                PositionDescriptor = positionDescriptor;
                DeltaEventArgs = deltaEventArgs;

            }

            public ObjectPositionDescriptor PositionDescriptor;
            public System.Windows.Input.ManipulationDeltaEventArgs DeltaEventArgs;

        }

    }

}
