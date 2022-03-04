using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageLaka.Views.Dialogs;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace ImageLaka.Views
{
    public class ViewsManager: IRecipient<ApplyDialogMessage>
    {
        public void Receive(ApplyDialogMessage message)
        {
            var t = message.DialogType;
            switch (t)
            {
                case DialogType.OpenImage:
                    break;
            }
        }
    }

    public class ApplyDialogMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Toolkit.Mvvm.Messaging.Messages.ApplyDialogMessage`1" /> class.
        /// </summary>
        /// <param name="dialogType">Apply for a type of dialog.</param>
        public ApplyDialogMessage(DialogType dialogType) => this.DialogType = dialogType;

        /// <summary>Apply for a type of dialog.</summary>
        public DialogType DialogType { get; }
    }
}
