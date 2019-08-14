using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace CardMaker
{
    public interface ICardSetsCollection
    {
        IList<ITreeViewItemViewModel> GetPrintList();

        void AddCard(ITreeViewItemViewModel selectedItem);
        void AddSet();
        void RemoveCard(ITreeViewItemViewModel selectedItem);
        void RemoveSet(ITreeViewItemViewModel selectedItem);
        void RenameSet();
        void ExportCard(string fileName);
        void ImportCard(string fileName);
        void ExportSet(string fileName);
        void ImportSet(string fileName);
        void SaveWorkspace(string fileName);
        void OpenWorkspace(string fileName);
        void AppendWorkspace(string fileName);
        void PrintCards();
    }
}
