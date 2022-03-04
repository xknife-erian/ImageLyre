using ImageLaka.Views.Dialogs;

namespace ImageLaka.ViewModels;

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