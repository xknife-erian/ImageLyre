using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using WPFSample.Panes;

namespace WPFSample
{
    public class WorkbenchViewModel: ObservableRecipient
    {

        public WorkbenchViewModel(HistogramSampleViewModel histogramSampleVm, MatSampleViewModel matSampleVm)
        {
            HistogramSampleVm = histogramSampleVm;
            MatSampleVm = matSampleVm;
        }

        public HistogramSampleViewModel HistogramSampleVm { get; set; }
        public MatSampleViewModel MatSampleVm { get; set; }
    }
}
