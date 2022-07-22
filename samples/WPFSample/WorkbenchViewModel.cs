using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using WPFSample.Sub;

namespace WPFSample
{
    public class WorkbenchViewModel: ObservableRecipient
    {

        public WorkbenchViewModel(HistogramSampleViewModel histogramSampleVm)
        {
            HistogramSampleVm = histogramSampleVm;
        }

        public HistogramSampleViewModel HistogramSampleVm { get; set; }
    }
}
