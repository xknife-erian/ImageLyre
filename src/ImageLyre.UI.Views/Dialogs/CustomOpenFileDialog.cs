﻿using System;
using System.Windows;
using MvvmDialogs.FrameworkDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Ookii.Dialogs.Wpf;

namespace ImageLyric.UI.Views.Dialogs
{
    public class CustomOpenFileDialog : IFrameworkDialog
    {
        private readonly OpenFileDialogSettings _settings;
        private readonly VistaOpenFileDialog _openFileDialog;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomOpenFileDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings for the open file dialog.</param>
        public CustomOpenFileDialog(OpenFileDialogSettings settings)
        {
            this._settings = settings ?? throw new ArgumentNullException(nameof(settings));

            _openFileDialog = new VistaOpenFileDialog
            {
                AddExtension = settings.AddExtension,
                CheckFileExists = settings.CheckFileExists,
                CheckPathExists = settings.CheckPathExists,
                DefaultExt = settings.DefaultExt,
                FileName = settings.FileName,
                Filter = settings.Filter,
                FilterIndex = settings.FilterIndex,
                InitialDirectory = settings.InitialDirectory,
                Multiselect = settings.Multiselect,
                Title = settings.Title
            };
        }

        /// <summary>
        /// Opens a open file dialog with specified owner.
        /// </summary>
        /// <param name="owner">
        /// Handle to the window that owns the dialog.
        /// </param>
        /// <returns>
        /// true if user clicks the OK button; otherwise false.
        /// </returns>
        public bool? ShowDialog(Window owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));

            var result = _openFileDialog.ShowDialog(owner);

            // Update settings
            _settings.FileName = _openFileDialog.FileName;
            _settings.FileNames = _openFileDialog.FileNames;
            _settings.FilterIndex = _openFileDialog.FilterIndex;

            return result;
        }
    }
}
