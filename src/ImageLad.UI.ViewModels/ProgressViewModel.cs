using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ImageLad.UI.ViewModels
{
    public class ProgressViewModel : ObservableRecipient
    {
        private long _maximum;
        private long _current;
        private string _title;
        private string _message;
        private long _minimum;

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
    }
}
