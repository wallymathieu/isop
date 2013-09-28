using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Isop.Tests
{
    class FullConfiguration:IDisposable
    {
        public bool DisposeCalled = false;
        
        public IEnumerable<Type> Recognizes()
        {
            return new[] { typeof(MyController) };
        }
        /// <summary>
        /// GLOBAL!!
        /// </summary>
        /// <value>
        /// The global.
        /// </value>
        //
        public string Global {
            get;
            set;
        }
        [Required]
        public string GlobalRequired
        {
            get;
            set;
        }
        public object ObjectFactory(Type type)
        {
            return null;
        }
        public CultureInfo Culture
        {
            get{ return CultureInfo.GetCultureInfo("es-ES"); }
        }
        public bool RecognizeHelp{get{return true;}}
        public void Dispose()
        {
            DisposeCalled = true;
        }
        public Func<Type, string, CultureInfo, object> GetTypeconverter()
        {
            return TypeConverter;
        }
        public static object TypeConverter(Type t, string s, CultureInfo c){ return null; }
    }
}