using Microsoft.Toolkit.Mvvm.ComponentModel;
using MvvmDialogs;

namespace ImageLyric.UI.ViewModels
{
    public class ProgressViewModel : ObservableRecipient, IModalDialogViewModel
    {
        private long _maximum;
        private long _current;
        private string _title;
        private string _message;
        private long _minimum;
        private bool _canClosed;

        public long Minimum
        {
            get => _minimum;
            set => SetProperty(ref _minimum, value);
        }

        public long Maximum
        {
            get => _maximum;
            set => SetProperty(ref _maximum, value);
        }

        public long Current
        {
            get => _current;
            set => SetProperty(ref _current, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public bool CanClosed
        {
            get => _canClosed;
            set => SetProperty(ref _canClosed, value);
        }

        #region Implementation of IModalDialogViewModel

        /// <summary>
        /// Gets the dialog result value, which is the value that is returned from the
        /// <see cref="M:MvvmDialogs.IDialogService.ShowDialog(System.ComponentModel.INotifyPropertyChanged,MvvmDialogs.IModalDialogViewModel)" /> and <see cref="M:MvvmDialogs.IDialogService.ShowDialog``1(System.ComponentModel.INotifyPropertyChanged,MvvmDialogs.IModalDialogViewModel)" />
        /// methods.
        /// </summary>
        public bool? DialogResult { get; set; }

        #endregion

        public void Finish()
        {
            DialogResult = true;
            CanClosed = true;
        }
    }
}
