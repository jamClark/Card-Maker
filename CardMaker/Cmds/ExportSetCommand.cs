using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CardMaker
{
    public class ExportSetCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            ICardSetsCollection collection = parameter as ICardSetsCollection;
            if (collection != null)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "Document"; // Default file name
                dlg.DefaultExt = ".cardset.xml"; // Default file extension
                dlg.Filter = "Card Set documents (.cardset.xml)|*.cardset.xml"; // Filter files by extension

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    collection.ExportSet(dlg.FileName);
                }
                //else
                //{
                //    MessageBox.Show("There was an error saving the card set. " + dlg.FileName);
                //}
            }
        }
    }
}