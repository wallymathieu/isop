using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;

using System.Text;
using Isop.Gui.Adapters;

namespace Isop.Wpf.Tests
{
    [TestFixture]
    public class BuildClientTests
    {
        private string _httpUrl;
        private string _invalidUrl;
        private string _validUrl;

        [SetUp]
        public void SetUp()
        {
            _validUrl = @"C:\Windows\notepad.exe";
            _invalidUrl = @"C:\Windows\XXX.exe";
            _httpUrl = @"http://www.google.com";
        }

        [Test]
        public void ReportsThatItCanUse()
        {
            Assert.True(BuildClient.CanLoad(_validUrl), "Valid url");
            Assert.False(BuildClient.CanLoad(_invalidUrl), "Invalid url");
            Assert.False(BuildClient.CanLoad(_httpUrl), "http url");
        }

    }
}
