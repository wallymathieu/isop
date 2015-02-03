using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

using System.Text;
using Isop.Gui.Adapters;

namespace Isop.Wpf.Tests
{
    [TestFixture]
    public class JsonClientTests
    {
        private string _fileSystemUrl;
        private string _invalidUrl;
        private string _validUrl;

        [SetUp]
        public void SetUp()
        {
            _validUrl = @"http://www.google.com";
            _invalidUrl = @"ftp://www.google.com";
            _fileSystemUrl = @"C:\Windows\notepad.exe";

        }

        [Test]
        public void ReportsThatItCanUse()
        {
            Assert.True(JsonClient.CanLoad(_validUrl),"Valid url");
            Assert.False(JsonClient.CanLoad(_invalidUrl), "Invalid url");
            Assert.False(JsonClient.CanLoad(_fileSystemUrl), "Filesystem url");

        }
    }
}
