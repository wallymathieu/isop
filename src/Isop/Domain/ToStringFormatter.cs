using System;
using System.Globalization;
using System.Collections.Generic;
using Isop.Infrastructure;
using System.Collections;

namespace Isop.Domain
{
	class ToStringFormatter 
	{
        public IEnumerable<string> Format(object retval)
        {
            if (retval != null)
            {
                if (retval is string)
                {
                    yield return (retval as string);
                }
                else if (retval is IEnumerable)
                {
                    foreach (var item in retval as IEnumerable)
                    {
                        foreach (var formatted in Format(item))
                        {
                            yield return formatted;
                        }
                    }
                }
                else
                {
                    yield return (retval.ToString());
                }
            }
        }
    }
}

