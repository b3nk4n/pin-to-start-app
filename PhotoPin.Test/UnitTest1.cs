using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace PhotoPin.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestGetFileNameWithoutExtension()
        {
            string p = Path.GetFileNameWithoutExtension("test.txt");
            Assert.IsNotNull(p);
        }

        [TestMethod]
        public void TestGetFileNameWithoutExtensionWithSpace()
        {
            char[] x = Path.GetInvalidPathChars();
            Console.WriteLine(x);
        }
    }
}
