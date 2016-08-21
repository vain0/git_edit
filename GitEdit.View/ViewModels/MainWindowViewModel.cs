using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using GitEdit.View.Properties;

namespace GitEdit.View.ViewModel
{
    public class MainWindowViewModel
        : ViewModelBase
    {
        public MainWindowViewModel(IMainWindow view)
        {
            _view = view;

            Editor.ModificationIndicatorChanged +=
                (sender, e) => NotifyPropertyChanged(nameof(Title));
            Editor.Document.FileNameChanged +=
                (sender, e) => NotifyPropertyChanged(nameof(Title));

            // Open the given file
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                var file = new FileInfo(args[1]);
                if (file.Exists) { OpenFile(file); }
            }
        }

        private IMainWindow _view;

        private MyTextEditor Editor =>
            _view.Editor;

        private Rect _rect = Settings.Default.MainWindowRect;
        public Rect Rect
        {
            get { return _rect; }
            set { SetProperty(ref _rect, value); }
        }

        public string FontFamily =>
            Settings.Default.FontFamily;

        public int FontSize =>
            Settings.Default.FontSize;

        public string Title
        {
            get
            {
                var doc = Editor.Document;
                if (doc == null) return Constant.AppName;
                var fileName =
                    string.IsNullOrEmpty(doc.FileName)
                    ? "untitled"
                    : Path.GetFileName(doc.FileName);
                var indicator =
                    _view.Editor.IsModified ? " *" : "";
                return string.Format("{0}{1} | {2}", fileName, indicator, Constant.AppName);
            }
        }

        public string SyntaxName =>
            Editor.SyntaxHighlighting.Name;

        public string EncodingName =>
            Editor.Encoding.EncodingName;

        private void UpdateStatusBar()
        {
            foreach (var name in new string[] { nameof(SyntaxName), nameof(EncodingName) })
            {
                NotifyPropertyChanged(name);
            }
        }

        public void OpenFile(FileInfo file)
        {
            Editor.LoadFile(file);
            UpdateStatusBar();
        }

        public void Save()
        {
            var currentFileName = _view.CurrentFileName;
            var fileInfoOrNull =
                string.IsNullOrEmpty(currentFileName)
                ? _view.GetSaveFileOrNull()
                : new FileInfo(currentFileName);
            if (fileInfoOrNull == null) return;

            Editor.SaveFile(fileInfoOrNull);
        }

        private RelayCommand _saveQuitCommand;
        public ICommand SaveQuitCommand =>
            _saveQuitCommand
            ?? (_saveQuitCommand = new RelayCommand(_ => SaveQuit()));

        public void SaveQuit()
        {
            Save();
            _view.Quit();
        }

        private RelayCommand _clearQuitCommand;
        public ICommand ClearQuitCommand =>
            _clearQuitCommand
            ?? (_clearQuitCommand = new RelayCommand(_ => ClearQuit()));

        public void ClearQuit()
        {
            Editor.Document.Text = "";
            SaveQuit();
        }
    }

    public interface IMainWindow
    {
        MyTextEditor Editor { get; }
        string CurrentFileName { get; }
        FileInfo GetSaveFileOrNull();
        void Quit();
    }
}
