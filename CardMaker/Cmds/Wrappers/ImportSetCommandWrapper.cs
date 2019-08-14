using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CardMaker
{
    public class ImportSetCommandWrapper
    {
        public ICommand Cmd{ get; set;}

        public ImportSetCommandWrapper()
        {
            Cmd = new ImportSetCommand();
        }
    }
}
