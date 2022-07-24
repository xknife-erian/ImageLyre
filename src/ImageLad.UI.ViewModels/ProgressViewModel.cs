using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using MvvmDialogs;

namespace ImageLad.UI.ViewModels
{
    public class ProgressViewModel : ObservableRecipient, IModalDialogViewModel
    {
        private long _maximum;
        private long _current;
        private string _title;
        private string _message;
        private long _minimum;
        private bool _closer;

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

        public bool Closer
        {
            get => _closer;
            set => SetProperty(ref _closer, value);
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
            Closer = true;
        }
    }
}
