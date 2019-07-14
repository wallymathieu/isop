using System;
using System.Globalization;
using System.Collections.Generic;
using Isop.Infrastructure;
using System.Collections;

namespace Isop.Domain
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

