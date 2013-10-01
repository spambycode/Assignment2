/* PROJECT:  Asign 1 (C#)            PROGRAM: SetupProgram
 * AUTHOR: George Karaszi   
 *******************************************************************************/

using System;
using System.IO;

using SharedClassLibrary;

namespace SetupProgram
{
    public class SetupProgram
    {
        public static void Main(string[] args)
        {
            int RecordCount   = 0;
            int RecordSuccess = 0;
            int RecordError   = 0;

            string fileNameSuffix;
            if (args.Length > 0)
            {
                fileNameSuffix = args[0];
            }
            else
            {
                fileNameSuffix = "Just26";
            }

            DeleteFile("Maindata.txt");

            // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            string FileName = "RawData" + fileNameSuffix + ".csv";

            SharedClassLibrary.UserInterface UI = new UserInterface();
            SharedClassLibrary.RawData RD = new RawData(UI, FileName);
            SharedClassLibrary.MainData MD = new MainData(UI);

            UI.WriteToLog("\n***************Setup App Start***************\n");
            while (RD.ReadOneCountry() != true)
            {
                if(MD.StoreOneCountry(RD))
                {
                    RecordSuccess++;
                }
                else
                {
                    RecordError++;
                }

                RecordCount++;
            }

            UI.WriteToLog(string.Format("Setup complete: {0} Total records processed ({1} Successes and {2} Errors) ", 
                            RecordCount, RecordSuccess, RecordError));

            UI.WriteToLog("\n***************Setup App END***************\n");
            MD.FinishUp();
            RD.FinishUp();
            UI.FinishUp(true, false);



        }
        //*********************** PRIVATE METHODS ********************************


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
    }
}
