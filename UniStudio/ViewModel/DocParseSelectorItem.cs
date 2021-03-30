using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Plugins.Shared.Library.UiAutomation;
using System.Windows;
using Uni.Core;

namespace UniStudio.ViewModel
{
    public class DocParseSelectorItem : ViewModelBase
    {
        private DocParseViewModel m_vm;

        public DocParseSelectorItem(DocParseViewModel vm)
        {
            this.m_vm = vm;
        }


        public const string ParamPropertyName = "Param";
        private string _paramProperty = "";
        public string Param
        {
            get
            {
                return _paramProperty;
            }
            set
            {
                if (_paramProperty == value)
                {
                    return;
                }

                _paramProperty = value;
                RaisePropertyChanged(ParamPropertyName);
            }
        }


        public const string SelectorPropertyName = "Selector";
        private string _selectorProperty = "";
        public string Selector
        {
            get
            {
                return _selectorProperty;
            }
            set
            {
                if (_selectorProperty == value)
                {
                    return;
                }

                _selectorProperty = value;

                if (!string.IsNullOrEmpty(_selectorProperty))
                {
                    SelectorStatusImgSource = "pack://application:,,,/Resource/Image/Windows/DocParse/complete.png";
                    SelectorStatusToolTip = "元素已完成录制";
                }
                else
                {
                    SelectorStatusImgSource = "pack://application:,,,/Resource/Image/Windows/DocParse/warning.png";
                    SelectorStatusToolTip = "元素尚未录制";
                }

                RaisePropertyChanged(SelectorPropertyName);
            }
        }

        public const string SelectorStatusImgSourcePropertyName = "SelectorStatusImgSource";
        private string _selectorStatusImgSourceProperty = "pack://application:,,,/Resource/Image/Windows/DocParse/warning.png";
        public string SelectorStatusImgSource
        {
            get
            {
                return _selectorStatusImgSourceProperty;
            }
            set
            {
                if (_selectorStatusImgSourceProperty == value)
                {
                    return;
                }

                _selectorStatusImgSourceProperty = value;
                RaisePropertyChanged(SelectorStatusImgSourcePropertyName);
            }
        }


        public const string SelectorStatusToolTipPropertyName = "SelectorStatusToolTip";
        private string _selectorStatusToolTipProperty = "元素尚未录制";
        public string SelectorStatusToolTip
        {
            get
            {
                return _selectorStatusToolTipProperty;
            }
            set
            {
                if (_selectorStatusToolTipProperty == value)
                {
                    return;
                }

                _selectorStatusToolTipProperty = value;
                RaisePropertyChanged(SelectorStatusToolTipPropertyName);
            }
        }


        public const string ScreenshotFileNamePropertyName = "ScreenshotFileName";
        private string _screenshotFileNameProperty = "";
        public string ScreenshotFileName
        {
            get
            {
                return _screenshotFileNameProperty;
            }
            set
            {
                if (_screenshotFileNameProperty == value)
                {
                    return;
                }

                _screenshotFileNameProperty = value;
                RaisePropertyChanged(ScreenshotFileNamePropertyName);
            }
        }


        public const string OriginActivityDescriptionPropertyName = "OriginActivityDescription";
        private ActivityDescription _originActivityDescriptionProperty;
        public ActivityDescription OriginActivityDescription
        {
            get
            {
                return _originActivityDescriptionProperty;
            }
            set
            {
                if (_originActivityDescriptionProperty == value)
                {
                    return;
                }

                _originActivityDescriptionProperty = value;
                RaisePropertyChanged(OriginActivityDescriptionPropertyName);
            }
        }


        private RelayCommand _startSelectElementCommand;
        public RelayCommand StartSelectElementCommand
        {
            get
            {
                return _startSelectElementCommand
                    ?? (_startSelectElementCommand = new RelayCommand(
                    () =>
                    {
                        UiElement.OnSelected = UiElement_OnSelected;
                        UiElement.StartElementHighlight();
                    }));
            }
        }


        private void UiElement_OnSelected(UiElement uiElement)
        {
            this.Selector = uiElement.Selector.ToString();
            this.ScreenshotFileName = uiElement.CaptureInformativeScreenshotToFile();

            Application.Current.MainWindow.Activate();
        }

    }
}
