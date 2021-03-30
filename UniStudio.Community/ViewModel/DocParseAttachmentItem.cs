using GalaSoft.MvvmLight;

namespace UniStudio.Community.ViewModel
{
    public class DocParseAttachmentItem : ViewModelBase
    {
        private DocParseViewModel m_vm;

        public DocParseAttachmentItem(DocParseViewModel vm)
        {
            this.m_vm = vm;
        }


        public const string NamePropertyName = "Name";
        private string _nameProperty = "";
        public string Name
        {
            get
            {
                return _nameProperty;
            }
            set
            {
                if (_nameProperty == value)
                {
                    return;
                }

                _nameProperty = value;
                RaisePropertyChanged(NamePropertyName);
            }
        }


        public const string PathPropertyName = "Path";
        private string _pathProperty = "";
        public string Path
        {
            get
            {
                return _pathProperty;
            }
            set
            {
                if (_pathProperty == value)
                {
                    return;
                }

                _pathProperty = value;
                RaisePropertyChanged(PathPropertyName);
            }
        }


        public const string IsSelectedPropertyName = "IsSelected";
        private bool _isSelectedProperty = false;
        public bool IsSelected
        {
            get
            {
                return _isSelectedProperty;
            }
            set
            {
                if (_isSelectedProperty == value)
                {
                    return;
                }

                _isSelectedProperty = value;
                RaisePropertyChanged(IsSelectedPropertyName);

                // 附件点击时处理相应界面变化
                if (value)
                {
                    m_vm.IsAddAttachmentEnabled = true;
                    m_vm.IsRemoveAttachmentEnabled = true;

                    m_vm.CurrentSelectedAttachmentItemName = this.Name;
                    m_vm.CurrentSelectedAttachmentItemPath = this.Path;
                }
            }
        }

    }
}
