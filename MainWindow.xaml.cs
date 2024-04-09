using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PendingFileRenameOperationsViewer
{
    public partial class MainWindow : Window
    {
        private readonly RenameOperationStore store;

        private readonly Action<object, RoutedEventArgs> SelectSourceFile;
        private readonly Action<object, RoutedEventArgs> SelectSourceFolder;
        private readonly Action<object, RoutedEventArgs> SelectDestFile;
        private readonly Action<object, RoutedEventArgs> SelectDestFolder;

        public MainWindow()
        {
            InitializeComponent();

            store = new RenameOperationStore();
            OperationsGrid.ItemsSource = store.collection;

            SelectSourceFile = CreateSelectAction("Source", false);
            SelectSourceFolder = CreateSelectAction("Source", true);
            SelectDestFile = CreateSelectAction("Dest", false);
            SelectDestFolder = CreateSelectAction("Dest", true);
        }

        private Action<object, RoutedEventArgs> CreateSelectAction(string propName, bool isFolder)
        {
            return (object sender, RoutedEventArgs e) =>
            {
                var op = (sender as Button).DataContext as RenameOperation;
                var prop = typeof(RenameOperation).GetProperty(propName);

                var dialog = new CommonOpenFileDialog
                {
                    EnsureFileExists = false,
                    EnsurePathExists = false,
                    IsFolderPicker = isFolder,
                    InitialDirectory = op != null
                        ? Path.GetDirectoryName((string)prop.GetValue(op, null))
                        : null
                };

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    op ??= store.Create();
                    prop.SetValue(op, dialog.FileName);
                }
            };
        }

        private void PerformOperation(object sender, RoutedEventArgs e)
        {
            var op = (sender as Button).DataContext as RenameOperation;

            if (op == null) return;

            try
            {
                if (op.Dest != string.Empty)
                {
                    File.Move(op.Source, op.Dest);
                } else
                {
                    File.Delete(op.Source);
                }

                store.collection.Remove(op);

                MessageBox.Show(
                    "Sucessfully performed operation.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch
            {
                MessageBox.Show(
                    "Failed to perform operation.",
                    "Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }
        private void DeleteOperation(object sender, RoutedEventArgs e)
        {
            var op = (sender as Button).DataContext as RenameOperation;

            if (op == null) return;

            store.collection.Remove(op);
        }
    }
}