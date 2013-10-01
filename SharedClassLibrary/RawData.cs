/* PROJECT:  Asign 1 (C#)            PROGRAM: RawData class
 * AUTHOR: George Karaszi   
 *******************************************************************************/

using System;
using System.IO;

namespace SharedClassLibrary
{
    public class RawData
    {
        //**************************** PRIVATE DECLARATIONS ************************
        private StreamReader rawDataFile;
        private UserInterface logFile;
        private string filename;

        //**************************** PUBLIC GET/SET METHODS **********************

        public string ID { get; set; }
        public string CODE { get; set; }
        public string NAME { get; set; }
        public string CONTINENT { get; set; }
        public string REGION { get; set; }
        public string SURFACEAREA { get; set; }
        public string YEAROFINDEP { get; set; }
        public string POPULATION { get; set; }
        public string LIFEEXPECTANCY { get; set; }
        

        //**************************** PUBLIC CONSTRUCTOR(S) ***********************

        /// <summary>
        /// Assign raw data via constructor.
        /// </summary>
        /// <param name="recordSplit">Split fields of record</param>
        public RawData(string[] recordSplit)
        {
            ID = recordSplit[0];
            CODE = recordSplit[1];
            NAME = recordSplit[2];
            CONTINENT = recordSplit[3];
            REGION = recordSplit[4];
            SURFACEAREA = recordSplit[5];
            YEAROFINDEP = recordSplit[6];
            POPULATION = recordSplit[7];
            LIFEEXPECTANCY = recordSplit[8];
        }

        public RawData(UserInterface LogFile, string filename)
        {
            try
            {
                logFile = LogFile;

                this.filename = filename;
                rawDataFile = new StreamReader(filename);
                logFile.WriteToLog("Open " + filename + " File");
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(0);
            }
        }
        //**************************** PUBLIC SERVICE METHODS **********************

        /// <summary>
        /// Reads one country from the rawdata file.
        /// </summary>
        /// <returns>EOF status</returns>
        public bool ReadOneCountry()
        {
            if (rawDataFile.EndOfStream != true)
            {
                var line = rawDataFile.ReadLine();
                var split = line.Split(',');


                //Asign varaibles from the read record
                ID   = split[0];
                CODE = split[1];
                NAME = split[2];
                CONTINENT = split[3];
                REGION = split[4];
                SURFACEAREA = split[5];
                YEAROFINDEP = split[6];
                POPULATION = split[7];
                LIFEEXPECTANCY = split[8];


                return false;

            }
            return true;
        }

        /// <summary>
        /// Close opened files
        /// </summary>
        public void FinishUp()
        {
            rawDataFile.Close();
            logFile.WriteToLog("Closed " + filename + " file");
        }


        //**************************** PRIVATE METHODS *****************************

    }
}
