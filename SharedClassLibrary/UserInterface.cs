/* PROJECT: Asign 1 (C#)         CLASS: UserInterface
 * AUTHOR:George Karaszi
 * FILES ACCESSED: TransData*.txt and log.txt
 * DESCRIPTION: Opens and writes to log file. Opens and processes transdata
 *******************************************************************************/

using System;
using System.IO;

namespace SharedClassLibrary
{
    public class UserInterface
    {
        //**************************** PRIVATE DECLARATIONS ************************
        private StreamReader transDataFile;
        private StreamWriter logFile;
        private string TransFileName;

        //**************************** PUBLIC GET/SET METHODS **********************


        //**************************** PUBLIC CONSTRUCTOR(S) ***********************
        public UserInterface(bool OpenLog, bool OpenTrans, string TransFileName = "Transdata.txt")
        {
            Console.WriteLine("OK, UserInterface object created");
            this.TransFileName = TransFileName;

            if(OpenLog)
            {
                logFile = new StreamWriter("Log.txt", true);
                WriteToLog("Log File Open");
            }

            if (OpenTrans)
            {
                transDataFile = new StreamReader(TransFileName);
                WriteToLog(TransFileName + " File Open");
            }
        }

        public UserInterface()
        {
            logFile = new StreamWriter("Log.txt", true);
            WriteToLog("Log File Open");
        }
        //**************************** PUBLIC SERVICE METHODS **********************


        //--------------------------------------------------------------------------
        /// <summary>
        /// Write to the log file
        /// </summary>
        /// <param name="input">string that wants to be inputted to the log file</param>
        public void WriteToLog(string input)
        {
            if(logFile != null)
            {
                logFile.WriteLine(input);
            }
            else
            {
                Console.WriteLine("Log file is not opened");
            }
        }


        //---------------------------------------------------------------------------
        /// <summary>
        /// Reads a line in transdata.
        /// </summary>
        /// <param name="query">Query data of what to return to calle</param>
        /// <returns>returns EOF state</returns>

        public bool GetOneTransdata(out string query)
        {
            query = "";

            if (transDataFile != null)
            {
                if (transDataFile.EndOfStream == false)
                {
                    query = transDataFile.ReadLine();

                    return false;
                }
            }
            return true;
        }



        //----------------------------------------------------------------------------
        /// <summary>
        /// Close open files used
        /// </summary>
        /// <param name="CloseTransData">Trans data file</param>
        /// <param name="CloseLog">Close opened log file</param>
        public void FinishUp(bool CloseLog, bool CloseTransData)
        {
            if(CloseTransData && transDataFile != null)
            {
                WriteToLog(TransFileName + " File Closed");
                transDataFile.Close();
                transDataFile = null;
            }

            if(CloseLog)
            {
                WriteToLog("Log File Closed"); ;
                logFile.Close();
                logFile = null;
            }
        }

        //**************************** PRIVATE METHODS *****************************


    }
}
