using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

using Youi;
using static Youi.YouiSorter;

namespace UnitTest
{
    [TestClass]
    public class TestYoui
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {

        }

        [TestMethod]
        [TestCategory("TestIsPhoneNumber")]
        public void TestNullPhoneNumber()
        {
            Assert.IsFalse(YouiSorter.IsPhoneNumber(null));
        }

        [TestMethod]
        [TestCategory("TestIsPhoneNumber")]
        public void TestZeroLengthPhoneNumber()
        {
            Assert.IsFalse(YouiSorter.IsPhoneNumber(""));
        }

        [TestMethod]
        [TestCategory("TestIsPhoneNumber")]
        public void TestIsCorrectLengthPhoneNumber()
        {
            Assert.IsFalse(YouiSorter.IsPhoneNumber("012345"));
            Assert.IsTrue(YouiSorter.IsPhoneNumber("0123456"));
            Assert.IsTrue(YouiSorter.IsPhoneNumber("01234567"));
            Assert.IsFalse(YouiSorter.IsPhoneNumber("012345678"));
        }

        [TestMethod]
        [TestCategory("TestIsPhoneNumber")]
        public void TestHasAlphaCharacters()
        {
            Assert.IsFalse(YouiSorter.IsPhoneNumber("a1234567"));
            Assert.IsFalse(YouiSorter.IsPhoneNumber("0123&4567"));
        }

        [TestMethod]
        [TestCategory("TestParsing")]
        public void TestEmptyAndNullStrings()
        {
            string[] data = { "", null, "First,Last,My Address, 01234567" };
            var result = YouiSorter.ParseData(data).ToArray();

            Assert.IsTrue(result.Length == 1);
            Assert.IsTrue(result[0].FirstName == "First");
            Assert.IsTrue(result[0].LastName == "Last");
            Assert.IsTrue(result[0].Address == "My Address");
            Assert.IsTrue(result[0].PhoneNumber == "01234567");
        }

        [TestMethod]
        [TestCategory("TestSorting")]
        public void TestSortByStreetName()
        {
            Customer[] source = {
                                    new Customer { FirstName = "Alex", LastName = "Jones", Address = "1 3_My Address Street", PhoneNumber = "012346789" },
                                    new Customer { FirstName = "Bobi", LastName = "Greeg", Address = " 2 2_My Address Street", PhoneNumber = "012346789" },
                                    new Customer { FirstName = "Simi", LastName = "Swart", Address = "", PhoneNumber = "012346789" },
                                    new Customer { FirstName = "Amii", LastName = "Blart", Address = "3 1_My Address Street", PhoneNumber = "012346789" }
                                };
            var result = YouiSorter.SortByStreetName(source).ToArray();

            Assert.IsTrue(result.Length == 3);
            Assert.IsTrue(result[0].Address == "3 1_My Address Street");
            Assert.IsTrue(result[1].Address == " 2 2_My Address Street");
            Assert.IsTrue(result[2].Address == "1 3_My Address Street");
        }

        [TestMethod]
        [TestCategory("TestSorting")]
        public void TestGroupAndSortNames()
        {
            Customer[] source = {
                                    new Customer { FirstName = "Alex", LastName = "Jones", Address = "1 3_My Address Street", PhoneNumber = "012346789" },
                                    new Customer { FirstName = "Bobi", LastName = "Greeg", Address = " 2 2_My Address Street", PhoneNumber = "012346789" },
                                    new Customer { FirstName = "Alex", LastName = "Bobi", Address = " 2 2_My Address Street", PhoneNumber = "012346789" },
                                    new Customer { FirstName = "Simi", LastName = "Swart", Address = "", PhoneNumber = "012346789" },
                                    new Customer { FirstName = "Amii", LastName = "Blart", Address = "3 1_My Address Street", PhoneNumber = "012346789" },
                                    new Customer { FirstName = "Amii", LastName = "Frank", Address = "3 1_My Address Street", PhoneNumber = "012346789" },
                                    new Customer { FirstName = "Amii", LastName = "Jones", Address = "3 1_My Address Street", PhoneNumber = "012346789" }

                                };
            var result = YouiSorter.GroupAndSortByName(source).ToArray();

            Assert.IsTrue(result.Length == 10);            
            Assert.IsTrue(result[0] == "Blart : 1");
            Assert.IsTrue(result[1] == "Bobi : 1");
            Assert.IsTrue(result[2] == "Bobi : 1");
            Assert.IsTrue(result[3] == "Frank : 1");
            Assert.IsTrue(result[4] == "Greeg : 1");
            Assert.IsTrue(result[5] == "Simi : 1");
            Assert.IsTrue(result[6] == "Swart : 1");
            Assert.IsTrue(result[7] == "Alex : 2");
            Assert.IsTrue(result[8] == "Jones : 2");
            Assert.IsTrue(result[9] == "Amii : 3");
        }

        [TestMethod]
        [TestCategory("TestSorting")]
        public void TestFirstAndLastNamesAreGroupedSeparately()
        {
            Customer[] source = {
                                    new Customer { FirstName = "Bobi", LastName = "Greeg", Address = " 2 2_My Address Street", PhoneNumber = "012346789" },
                                    new Customer { FirstName = "Alex", LastName = "Bobi", Address = " 2 2_My Address Street", PhoneNumber = "012346789" },
                                };
            var result = YouiSorter.GroupAndSortByName(source).ToArray();

            Assert.IsTrue(result.Length == 4);
            Assert.IsTrue(result[0] == "Alex : 1");
            Assert.IsTrue(result[1] == "Bobi : 1");
            Assert.IsTrue(result[2] == "Bobi : 1");
            Assert.IsTrue(result[3] == "Greeg : 1");
        }

        [TestMethod]
        [TestCategory("TestSorting")]
        public void TestCanHandleEmptyAndNullName()
        {
            Customer[] source = {
                                    new Customer { FirstName = "", LastName = "Greeg", Address = " 2 2_My Address Street", PhoneNumber = "012346789" },
                                    new Customer { FirstName = null, LastName = "Game", Address = " 2 2_My Address Street", PhoneNumber = "012346789" },
                                    new Customer { FirstName = "James", LastName = null, Address = " 2 2_My Address Street", PhoneNumber = "012346789" },
                                    new Customer { FirstName = "Alex", LastName = "Bobi", Address = " 2 2_My Address Street", PhoneNumber = "012346789" },
                                    new Customer { FirstName = "Alex", LastName = "", Address = " 2 2_My Address Street", PhoneNumber = "012346789" },
                                };
            var result = YouiSorter.GroupAndSortByName(source).ToArray();

            Assert.IsTrue(result.Length == 7);
            Assert.IsTrue(result[0] == "Bobi : 1");
            Assert.IsTrue(result[1] == "Game : 1");
            Assert.IsTrue(result[2] == "Greeg : 1");
            Assert.IsTrue(result[3] == "James : 1");
            Assert.IsTrue(result[4] == " : 2");
            Assert.IsTrue(result[5] == " : 2");
            Assert.IsTrue(result[6] == "Alex : 2");
        }

    }
}

