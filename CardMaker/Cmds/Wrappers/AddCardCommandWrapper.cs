using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CardMaker
{
    public class AddCardCommandWrapper
    {
        public ICommand Cmd{ get; set;}

        public AddCardCommandWrapper()
        {
            Cmd = new AddCardCommand();
        }
    }
}
