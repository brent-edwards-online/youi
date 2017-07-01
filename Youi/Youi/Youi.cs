namespace Youi
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Linq;

    public class YouiSorter
    {
        private const int PhoneNumberColumnIndex = 3;
        private const string DataInputFile = @".\data.csv";
        private const string OutputFile1 = @".\output1.txt";
        private const string OutputFile2 = @".\output2.txt";
        private const string RegularExpression = "^[0-9]{7,8}$"; // Basic RegEx for a phone number
        static private readonly Regex RegularExpressionForPhoneNumber = new Regex(RegularExpression, RegexOptions.IgnoreCase);

        public class Customer
        {
            public string FirstName;
            public string LastName;
            public string Address;
            public string PhoneNumber;

            public override string ToString()
            {
                return "[\n" +
                        "  FirstName : " + FirstName + "\n" +
                        "  LastName : " + LastName + "\n" +
                        "  Address : " + Address + "\n" +
                        "  PhoneNumber : " + PhoneNumber + "\n" +
                        "]";
            }
        }

        /// <summary>  
        ///     Phone number input sanity check
        /// </summary>  
        /// <param name="phoneNumber">
        ///     A string representing a phone numnber
        /// </param>
        /// <returns>
        ///     true if a recognised phone number format, false otherwise
        /// </returns>
        /// <remarks>
        ///     Probably overkill since was not part of requirements
        /// </remarks>
        static public bool IsPhoneNumber(string phoneNumber)
        {
            if (phoneNumber == null)
            {
                return false;
            }
            else
            {
                return RegularExpressionForPhoneNumber.IsMatch(phoneNumber);
            }
        }

        /// <summary>  
        ///     Validates a line of input data
        /// </summary>  
        /// <param name="values">
        ///     Array of strings extracted from the input line
        /// </param>
        /// <returns>
        ///     true if line is valid, false otherwise
        /// </returns>
        /// <remarks>
        ///     ASSUMPTION number of fields should be four
        ///     We really could ignore any fields > 3
        /// </remarks>
        static public bool IsValidLine(string[] values)
        {
            if (values == null)
            {
                return false;
            }

            if (values.Length != 4)
            {
                return false;
            }

            return true;
        }

        /// <summary>  
        ///     Exports a list of strings to a file
        /// </summary>  
        /// <param name="exportList">
        ///     List of strings to export
        /// </param>
        /// <param name="fileName">
        ///     The file name to export the list to
        /// </param>
        /// <returns>
        ///     true if successful, false otherwise
        /// </returns>
        static bool ExportList(IList<string> exportList, string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                using (var fs = File.OpenWrite(fileName))
                using (var writer = new StreamWriter(fs))
                {
                    foreach (var data in exportList)
                    {
                        writer.WriteLine(data);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to export");
                return false;
            }
        }

        /// <summary>  
        ///     Imports the file to process
        /// </summary>  
        /// <param name="fileName">
        ///     The file to import
        /// </param>
        /// <returns>
        ///     Contents of file as a list of strings
        /// </returns>
        /// <remarks>
        ///     Risk if large file and we run out of memory
        /// </remarks>
        static IList<string> ImportFile(string fileName)
        {
            IList<string> rawData = new List<string>();

            using (var fs = File.OpenRead(DataInputFile))
            using (var reader = new StreamReader(fs))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    rawData.Add(line);
                }
            }

            return rawData;
        }

        /// <summary>  
        ///     Parses the input data into Customer objects
        /// </summary>  
        /// <param name="data">
        ///     A list of comma delimited strings
        /// </param>
        /// <returns>
        ///     List of Customer obects
        /// </returns>
        /// <remarks>
        ///     Risk if large file and we run out of memory
        ///     Validating of PhoneNumber was done here but removed since not really a requirement
        /// </remarks>
        public static IList<Customer> ParseData(IList<string> data)
        {
            IList<Customer> customers = new List<Customer>();

            foreach (var line in data)
            {
                if(line == null)
                {
                    continue;
                }

                var values = line.Split(',');
                if (IsValidLine(values))
                {
                    var customer = new Customer
                    {
                        FirstName = values[0].Trim(),
                        LastName = values[1].Trim(),
                        Address = values[2].Trim(),
                        PhoneNumber = values[3].Trim()
                    };
                    customers.Add(customer);
                    Console.WriteLine(customer.ToString());
                }
            }
            return customers;
        }

        /// <summary>  
        ///     Sorts a list of customer objects by frequency of first and last names
        /// </summary>  
        /// <param name="customers">
        ///     List of customer objects
        /// </param>
        /// <returns>
        ///     List of strings containing name and frequency of occurance sort by frequency and then name
        /// </returns>
        /// <remarks>
        ///     Names are trimmed
        ///     ASSUMPTION order by frequency is ascending
        ///     ASSUMPTION we dont have to group first and last if they are the same
        ///     e.g. Jim Smith and Smith Jones - Smith is not grouped together 
        /// </remarks>
        static public IEnumerable<string> GroupAndSortByName(IList<Customer> customers)
        {
            var nameList = customers.GroupBy(c => c.FirstName == null ? "" : c.FirstName.Trim())
                                  .Select(n => new
                                  {
                                      Name = n.Key,
                                      Count = n.Count()
                                  }).ToList();

            nameList.AddRange(
                customers.GroupBy(c => c.LastName == null ? "" : c.LastName.Trim())
                                  .Select(n => new
                                  {
                                      Name = n.Key,
                                      Count = n.Count()
                                  }).ToList()

            );

            var result = nameList.OrderBy(c => c.Count).ThenBy(n => n.Name).Select(r => r.Name + " : " + r.Count);
            return result;
        }

        /// <summary>  
        ///     Sorts a list of customer objects by street name
        /// </summary>  
        /// <param name="customers">
        ///     List of customer objects
        /// </param>
        /// <returns>
        ///     List of customer objects sorted by street name
        /// </returns>
        /// <remarks>
        ///     Address is trimmed
        ///     ASSUMPTION the street name is everything after the the first word in the address field
        ///     ASSUMPTION sorting is ascending
        /// </remarks>
        static public IEnumerable<Customer> SortByStreetName(IList<Customer> customers)
        {
            var result = customers.Where(a => a.Address.Trim().Split(' ').Count() > 1)  // Assumption street name is text after first word in address
                            .Select(c => new { value = c, Street = c.Address.Substring(c.Address.IndexOf(' ')) })
                            .OrderBy(s => s.Street)
                            .Select(r => r.value);

            return result;
        }

        /// <summary>  
        ///     Applicaiton Entry Point
        /// </summary>  
        static void Main(string[] args)
        {
            if (File.Exists(DataInputFile))
            {
                var rawData = ImportFile(DataInputFile);

                var customers = ParseData(rawData);

                if (customers.Count > 0)
                {
                    // The first should show the frequency of the first and last names 
                    // ordered by frequency and then alphabetically. 
                    var nameList = GroupAndSortByName(customers);
                    ExportList(nameList.ToList(), OutputFile1);

                    // The second should show the addresses sorted alphabetically by street name.
                    var addressList = SortByStreetName(customers);
                    ExportList(addressList.Select(a => a.Address).ToList(), OutputFile2);

                    Console.WriteLine("Processing Complete. Press a key to finish");
                }
                else
                {
                    Console.WriteLine("Nothing to sort. Press a key to finish");
                }
            }
            else
            {
                Console.WriteLine("Input file does not exist. Make sure it is in the same folder as the executable: " + DataInputFile + "\nPress a key to finish");
            }

            Console.ReadKey();
        }
    }
}
