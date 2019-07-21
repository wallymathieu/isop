using System.Collections;
using System.Collections.Generic;

namespace Isop.CommandLine.Views
{
	class ToStringFormatter 
	{
        public IEnumerable<string> Format(object value)
        {
            if (value == null) yield break;
            switch (value)
            {
                case string s:
                    yield return s;
                    break;
                case IEnumerable enumerable:
                {
                    foreach (var item in enumerable)
                    {
                        foreach (var formatted in Format(item))
                        {
                            yield return formatted;
                        }
                    }
                    break;
                }
                default:
                    yield return value.ToString();
                    break;
            }
        }
    }
}

