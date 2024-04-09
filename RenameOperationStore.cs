using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PendingFileRenameOperationsViewer
{
    internal class RenameOperation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _storedSource;
        private string _storedDest;

        public string Source
        {
            get
            {
                return _storedSource;
            }
            set
            {
                _storedSource = value;
                NotifyPropertyChanged();
            }
        }

        public string Dest
        {
            get
            {
                return _storedDest;
            }
            set
            {
                _storedDest = value;
                NotifyPropertyChanged();
            }
        }

        public RenameOperation()
        {
            _storedSource = string.Empty;
            _storedDest = string.Empty;
        }

        public RenameOperation(string source, string dest)
        {
            _storedSource = source;
            _storedDest = dest;
        }

        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class RenameOperationStore
    {
        private static readonly string RegistryKey =
            "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Session Manager";
        private static readonly string RegistryValue = "PendingFileRenameOperations";

        public ObservableCollection<RenameOperation> collection;

        public RenameOperationStore()
        {
            collection = [];

            InitalizeCollection();

            collection.CollectionChanged += HandleCollectionChange;
        }

        private void InitalizeCollection()
        {
            string[] list = (string[])
                Registry.GetValue(RegistryKey, RegistryValue, Array.Empty<string>());

            collection.Clear();

            for (int i = 0; i < list.Length; i += 2)
            {
                var source = list[i];
                var dest = i + 1 == list.Length ? string.Empty : list[i + 1];

                var op = new RenameOperation(
                    source.Length > 0 ? source[4..] : source,
                    dest.Length > 0 ? dest[5..] : dest
                );

                // CollectionChanged event listener is not added yet
                Register(op);

                collection.Add(op);
            }
        }

        private void HandleCollectionChange(
            object? sender,
            NotifyCollectionChangedEventArgs e
        )
        {
            if (e.NewItems is not null)
            {
                foreach (var item in e.NewItems)
                {
                    Register(item as RenameOperation);
                }
            }

            Save();
        }

        private void HandleItemChange(object? sender, PropertyChangedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            var operations = new List<string>();

            foreach (var row in collection)
            {
                var source = row.Source;
                var dest = row.Dest;

                operations.Add(source.Length > 0 ? "\\??\\" + source : source);
                operations.Add(dest.Length > 0 ? "!\\??\\" + dest : dest);
            }

            Registry.SetValue(RegistryKey, RegistryValue, operations.ToArray());
        }

        private void Register(RenameOperation op)
        {
            op.PropertyChanged += HandleItemChange;
        }

        public RenameOperation Create()
        {
            var op = new RenameOperation();
            collection.Add(op);
            return op;
        }
    }
}
