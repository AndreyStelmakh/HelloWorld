using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace RegimST
{
    /// <summary>
    /// </summary>
    /// [TemplatePart(Name = "TextElement", Type = typeof(TextBlock))]
    //[TemplatePart(Name = "UpButtonElement", Type = typeof(RepeatButton))]
    //[TemplatePart(Name = "DownButtonElement", Type = typeof(RepeatButton))]
    //[TemplateVisualState(Name = "Wait", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Hidden", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    public partial class SomeControl : ContentControl
    {
        System.Threading.Timer _timer;
        Queue<BitmapImage> _imagesQueue;

        static SomeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SomeControl), new FrameworkPropertyMetadata(typeof(SomeControl)));

        }

        public SomeControl()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            _timer = new System.Threading.Timer((a) =>
            {
                if (_imagesQueue != null)
                {
                    if (_imagesQueue.Count != 0)
                    {
                        var image = _imagesQueue.Dequeue();

                        _imagesQueue.Enqueue(image);

                        #region Намеренно разбито на два, иначе транзишен вызываемый по .HiddenForeground не виден 

                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ForegroundImage = BackgroundImage;

                            ControlState = ControlStates.Normal;

                            BackgroundImage = image;

                        }));

                        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                        {
                            ControlState = ControlStates.HiddenForeground;

                        }));

                        #endregion

                    }

                }

            }, null, 0, 3000);

        }

        public ObservableCollection<BitmapImage> Images
        {
            get
            {
                return (ObservableCollection<BitmapImage>)GetValue(ImagesProperty);
            }
            set
            {
                SetValue(ImagesProperty, value);
            }
        }

        public static readonly DependencyProperty ImagesProperty =
            DependencyProperty.Register("Images", typeof(ObservableCollection<BitmapImage>), typeof(SomeControl), new PropertyMetadata((a,b)=>
            {
                var control = a as SomeControl;

                if (control != null)
                {
                    if (b.NewValue is IEnumerable<BitmapImage>)
                    {
                        control._imagesQueue = new Queue<BitmapImage>(b.NewValue as IEnumerable<BitmapImage>);

                    }
                    else
                    {
                        control._imagesQueue = null;

                    }

                }

            }));

        public BitmapImage BackgroundImage
        {
            get
            {
                return (BitmapImage)GetValue(BackgroundImageProperty);
            }
            set
            {
                SetValue(BackgroundImageProperty, value);
            }
        }

        public static readonly DependencyProperty BackgroundImageProperty =
            DependencyProperty.Register("BackgroundImage", typeof(BitmapImage), typeof(SomeControl), new PropertyMetadata());



        public BitmapImage ForegroundImage
        {
            get
            {
                return (BitmapImage)GetValue(ForegroundImageProperty);
            }
            set
            {
                SetValue(ForegroundImageProperty, value);
            }
        }

        public static readonly DependencyProperty ForegroundImageProperty =
            DependencyProperty.Register("ForegroundImage", typeof(BitmapImage), typeof(SomeControl));

        /// <summary>
        /// 
        /// </summary>
        enum ControlStates
        {
            /// <summary>
            /// видны оба изображения - и переднего плана и заднего плана
            /// </summary>
            Normal,
            /// <summary>
            /// только изображение заднего плана видимо
            /// </summary>
            HiddenForeground

        }

        ControlStates _controlState = ControlStates.Normal;

        private ControlStates ControlState
        {
            get
            {
                return _controlState;

            }

            set
            {
                _controlState = value;

                UpdateControlState(true);

            }

        }

        void UpdateControlState(bool useTransitions = false)
        {
            switch (ControlState)
            {
            case ControlStates.Normal:
                VisualStateManager.GoToState(this, "Normal", useTransitions);
                break;

            case ControlStates.HiddenForeground:
                VisualStateManager.GoToState(this, "HiddenForeground", useTransitions);

                break;

            }

        }

    }

}
