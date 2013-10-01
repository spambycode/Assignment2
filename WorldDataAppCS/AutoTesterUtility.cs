/* PROJECT:  Asign 1 (C#)            PROGRAM: AutoTesterUtility
 * AUTHOR: George Karaszi   
 *******************************************************************************/

using System;
using System.IO;

using SetupProgram;
using UserApp;
using PrettyPrintUtility;

namespace WorldDataAppCS
{
    class AutoTesterUtility
    {
        static void Main(string[] args)
        {

            // The 3 parallel arrays (all strings, including the N's) with
            //      - hard-coded SUFFIX values to designate which files to use
            //      - N's to limit how many records to display during testing
            // The dataFileSuffix is used for RawData*.csv, MainData*.bin,
            //      NameIndexBackup*.bin, CodeIndexBackup*.bin
            string[] dataFileSuffix = { "Just26", "Just26", "Dups", "" };
            string[] transFileSuffix = { "Typical", "Empty", "ForDups", "" };

            //Delete the SINGLE output Log.txt file (if it exists)
            DeleteFile("Log.txt");
            for (int i = 0; i < dataFileSuffix.Length; i++)
            {
                //Delete 3 other output files (if they exist)
                DeleteFile("MainData.txt");

               
                SetupProgram.SetupProgram.Main(new string[] { dataFileSuffix[i] });
                UserApp.UserApp.Main(new string[] {transFileSuffix[i] });
                PrettyPrintUtility.PrettyPrintUtility.Main(new string[] { dataFileSuffix[i]});
            }
        }
        //**************************************************************************
        private static bool DeleteFile(String fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                return true;
            }
            else
            {
                return false;
            }
        }

        //**************************************************************************
    }
}
