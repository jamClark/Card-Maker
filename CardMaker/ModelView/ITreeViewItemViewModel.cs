using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace CardMaker
{
    public interface ITreeViewItemViewModel : INotifyPropertyChanged
    {
        ObservableCollection<ITreeViewItemViewModel> Children { get; }
        string Name { get; }
        bool HasDummyChild { get; }
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        bool IsChecked { get; set; }
        bool IsLeaf { get; }
        ITreeViewItemViewModel Parent { get; }
    }
}
