/* PROJECT:  Asign 2 (C#)            PROGRAM: PrettyPrint (AKA ShowFilesUtility)
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
        static FileStream fMainDataFile;
        static FileStream fCollisionDataFile;
        static BinaryReader bMainDataFileReader;
        static BinaryReader bCollisionDataFileReader;
        static int _sizeOfHeaderRec = sizeof(short)*2;
        static int _sizeOfDataRec = 3 + 17 + 12 + sizeof(int) +sizeof(short) + sizeof(long) + 
                                    sizeof(float) + sizeof(int) + sizeof(short);


        public static void Main(string[] args)
        {

            fMainDataFile = new FileStream("MainData.bin", FileMode.Open);
            bMainDataFileReader = new BinaryReader(fMainDataFile);

            fCollisionDataFile = new FileStream("MainDataCollision.bin", FileMode.Open);
            bCollisionDataFileReader = new BinaryReader(fCollisionDataFile);

            string[] MainRecordList      = ReadFile(bMainDataFileReader, _sizeOfHeaderRec);

            string[] CollisionRecordList = ReadFile(bCollisionDataFileReader,
                                                    0);

            PrintResults(MainRecordList, CollisionRecordList);
            FinishUp();
          
        }


        private static string []ReadFile(BinaryReader fileReader, int headerLength)
        {
            char[] code;                           //Country code
            char[] name;                           //Name of country
            char[] continent;                      //What continent the country is located
            int  surfaceArea;                      //Size of the country
            short yearOfIndep;                     //What year they went independent
            long population;                       //Total population of the country
            float lifeExpectancy;                  //The average time someone is alive in the country
            int gnp;                               //Gross national product
            List<string> RecordCollection = new List<string>(); //List of formatted record strings


            for (long pos = headerLength; pos < fileReader.BaseStream.Length; pos += _sizeOfDataRec)
            {
                fileReader.BaseStream.Seek(pos, SeekOrigin.Begin);

                code = fileReader.ReadChars(3);

                if (code[0] == '\0')
                    continue;

                name           = fileReader.ReadChars(17);
                continent      = fileReader.ReadChars(12);
                surfaceArea    = fileReader.ReadInt32();
                yearOfIndep    = fileReader.ReadInt16();
                population     = fileReader.ReadInt64();
                lifeExpectancy = fileReader.ReadSingle();
                gnp            = fileReader.ReadInt32();

                string formatRecord = new string(code).PadRight(5, ' ') +
                                      new string(name).PadRight(20, ' ') +
                                      new string(continent).PadRight(18) +
                                      Convert.ToString(surfaceArea).PadRight(15, ' ') +
                                      Convert.ToString(yearOfIndep).PadRight(9, ' ') +
                                      Convert.ToString(population).PadRight(13, ' ') +
                                      Convert.ToString(lifeExpectancy).PadRight(8, ' ') +
                                      Convert.ToString(gnp);

                RecordCollection.Add(formatRecord);
            }

            return RecordCollection.ToArray();

        }

        private static void ReadCollisionFile()
        {

        }

        private static void FinishUp()
        {
            fMainDataFile.Close();
        }



        //------------------------------------------------------------------------------
        /// <summary>
        /// Formates the header to be displayed
        /// </summary>
        /// <returns>A ready to use string aligned in its columns</returns>
        private static string FormatHeader()
        {

            return      
                       "CODE".PadRight(5, ' ') +
                        "NAME".PadRight(20, ' ') +
                        "CONTINENT".PadRight(18, ' ') +
                        "AREA".PadRight(15, ' ') +
                        "INDEP".PadRight(9, ' ') +
                        "POPULATION".PadRight(13, ' ') +
                        "L.EXP".PadRight(12, ' ') +
                        "GNP";
        }

        //------------------------------------------------------------------------------
        /// <summary>
        /// Print the results from the formatted text
        /// </summary>
        private static void PrintResults(string[] MainDataList, string []CollisionDataList)
        {
            StreamWriter logFile = new StreamWriter("Log.txt", true);

            logFile.WriteLine("\n***************Pretty Print Start***************\n");
            logFile.WriteLine("Start of MainData.bin");
            logFile.WriteLine(FormatHeader());

            foreach (string s in MainDataList)
            {
                logFile.WriteLine(s);
            }

            logFile.WriteLine("End of MainData.bin");
            logFile.WriteLine("Start of MainDataCollision.bin");

            logFile.WriteLine(FormatHeader());

            foreach(string s in CollisionDataList)
            {
                logFile.WriteLine(s);
            }
            logFile.WriteLine("End of MainDataCollision.bin");

            logFile.WriteLine("\n**********End Of Pretty Print Utility**********\n");

            logFile.Close();

        }

    }
}
