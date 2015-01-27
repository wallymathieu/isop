using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isop.Gui
{
    class FromDroppedFile
    {
        internal string GetFileName(System.Windows.DragEventArgs e)
        {
            var data = e.Data.GetData("FileName");
            if (data is string[])
            {
                return ((string[])data)[0];
            }
            throw new NotImplementedException();
        }
    }
}
