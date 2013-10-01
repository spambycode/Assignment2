/* PROJECT:  Asign 1 (C#)            PROGRAM: PrettyPrint (AKA ShowFilesUtility)
 * AUTHOR: George Karaszi   
 *******************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace PrettyPrintUtility
{
    public class PrettyPrintUtility
    {
        static FileStream mainDataFile;
        static List<string> RecordList;
        static int _sizeOfHeaderRec = 3;
        static int _sizeOfDataRec = 71;

        public static void Main(string[] args)
        {
            if (File.Exists("MainData.txt"))
            {
                RecordList = new List<string>();
                mainDataFile = new FileStream("MainData.txt", FileMode.Open);
                HandleDataFile();
                PrintResults();
                FinishUp();

            }
            else
            {
                Console.WriteLine("Error MainData.txt does not exist!");
            }
          
        }

        private static void FinishUp()
        {
            mainDataFile.Close();
        }

        //------------------------------------------------------------------------------
        /// <summary>
        /// Handles the main data file and its records
        /// </summary>
        private static void HandleDataFile()
        {
            int RecordCount = ReadHeaderRec(); //Amount of records in file
            int id = 1;                        //Starting point in searching for ID's
            int RRN = 0;                       //RRN of file location
            byte[] QueryRecord;                //Record that was returned

            for(int i = 0; i < RecordCount;)
            {
                RRN = CalculateRRN(id++);
                QueryRecord = ReadOneRecord(RRN);

                //Check for empty record
                if(QueryRecord[0] != 0)
                {
                    RecordList.Add(FormatRecord(Encoding.UTF8.GetString(QueryRecord)));
                    ++i;
                }
                else
                {
                    RecordList.Add(string.Format("[{0}]".PadRight(7, ' ') +  
                                "Empty", Convert.ToString(RRN).PadLeft(3, '0')));
                }
            }

        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Returns the RRN value of the given parameter
        /// </summary>
        /// <param name="id">ID of query record</param>
        /// <returns></returns>
        private static int CalculateRRN(int id)
        {
            return id;
        }

        //---------------------------------------------------------------------------
        /// <summary>
        /// Obtain the offset to where the file pointer needs to point
        /// </summary>
        /// <param name="RRN">An ID to what record that needs to be obtained</param>
        /// <returns>offset to file positions</returns>
        private static int CalculateByteOffSet(int RRN)
        {
            return _sizeOfHeaderRec + ((RRN - 1) * _sizeOfDataRec);
        }


        //------------------------------------------------------------------------------
        /// <summary>
        /// Reads one record using the RRN
        /// </summary>
        /// <param name="RRN">File Location of the record</param>
        /// <returns>An array in byte form that came from the main data file</returns>
        private static byte[] ReadOneRecord(int RRN)
        {

            byte[] recordData = new byte[_sizeOfDataRec];
            int byteOffSet = CalculateByteOffSet(RRN);

            mainDataFile.Seek(byteOffSet, SeekOrigin.Begin);
            mainDataFile.Read(recordData, 0, recordData.Length);

            return recordData;
        }

        //------------------------------------------------------------------------------
        /// <summary>
        /// Reads the header that contains the amount of records inside
        /// </summary>
        /// <returns>Record amount number</returns>

        private static int ReadHeaderRec()
        {
            byte[] recordData = new byte[_sizeOfHeaderRec];

            mainDataFile.Seek(0, SeekOrigin.Begin);
            mainDataFile.Read(recordData, 0, recordData.Length);

            return Convert.ToInt32(Encoding.UTF8.GetString(recordData));
        }

        //------------------------------------------------------------------------------
        /// <summary>
        /// Formates the string to de aligned with its header columns
        /// </summary>
        /// <param name="record">record from main data</param>
        /// <returns>formatted string ready to be used</returns>
        private static string FormatRecord(string record)
        {
            string id;               //ID of record
            string code;             //Country code
            string name;             //Name of country
            string continent;        //What continent the country is located
            string region;           //What region the country is located
            string surfaceArea;      //Size of the country
            string yearOfIndep;      //What year they went independent
            string population;       //Total population of the country
            string lifeExpectancy;   //The average time someone is alive in the country
            int stringPos = 0;

            //Split record
            id             = GetSubStringRec(record, 3, ref stringPos);
            code           = GetSubStringRec(record, 3, ref stringPos);
            name           = GetSubStringRec(record, 17, ref stringPos);
            continent      = GetSubStringRec(record, 11, ref stringPos);
            region         = GetSubStringRec(record, 10, ref stringPos);
            surfaceArea    = GetSubStringRec(record, 8, ref stringPos);
            yearOfIndep    = GetSubStringRec(record, 5, ref stringPos); 
            population     = GetSubStringRec(record, 10, ref stringPos);
            lifeExpectancy = GetSubStringRec(record, 4, ref stringPos);




            return   "[" + id + "]".PadRight(3, ' ') +
                        id.PadRight(4, ' ') +
                        code.PadRight(5, ' ')+
                        name.PadRight(20, ' ') +
                        continent.PadRight(18)+
                        region.PadRight(15, ' ') +
                        surfaceArea.PadRight(15, ' ') +
                        yearOfIndep.PadRight(9, ' ') +
                        population.PadRight(13, ' ') +
                        lifeExpectancy;
        }


        //------------------------------------------------------------------------------
        /// <summary>
        /// Formates the header to be displayed
        /// </summary>
        /// <returns>A ready to use string aligned in its columns</returns>
        private static string FormatHeader()
        {

            return      "[RRN]".PadRight(7, ' ') +
                        "ID".PadRight(4, ' ') +
                        "CODE".PadRight(5, ' ') +
                        "NAME".PadRight(20, ' ') +
                        "CONTINENT".PadRight(18, ' ') +
                        "REGION".PadRight(15, ' ') +
                        "AREA".PadRight(15, ' ') +
                        "INDEP".PadRight(9, ' ') +
                        "POPULATION".PadRight(13, ' ') +
                        "L.EXP";
        }

        //------------------------------------------------------------------------------
        /// <summary>
        /// Split a string to get a necessary data field
        /// </summary>
        /// <param name="record">Full recorded wanted to split</param>
        /// <param name="recordlength">Length of desired record</param>
        /// <param name="strPosition">Where to start chopping</param>
        /// <returns>new sub-string record</returns>
        private static string GetSubStringRec(string record, int subRecordlength, ref int strPosition)
        {
            string subrec = record.Substring(strPosition, subRecordlength).Trim();
            strPosition += subRecordlength;

            return subrec;
        }

        //------------------------------------------------------------------------------
        /// <summary>
        /// Print the results from the formatted text
        /// </summary>
        private static void PrintResults()
        {
            StreamWriter logFile = new StreamWriter("Log.txt", true);

            logFile.WriteLine("\n***************Pretty Print Start***************\n");
            logFile.WriteLine(FormatHeader());

            foreach (string s in RecordList)
            {
                logFile.WriteLine(s);
            }

            logFile.WriteLine("\n**********End Of Pretty Print Utility**********\n");

            logFile.Close();

        }

    }
}
