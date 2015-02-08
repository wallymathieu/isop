using System;

namespace Isop.Gui
{
	class FromDroppedFile
	{
		internal string GetFileName(System.Windows.DragEventArgs e)
		{
			var data = e.Data.GetData("FileName");
			string[] array;
			if ((array = data as string[]) != null)
			{
				return array[0];
			}
			throw new NotImplementedException();
		}
	}
}
