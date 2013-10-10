/* PROJECT:  Asign 2 (C#)            PROGRAM: PrettyPrint (AKA ShowFilesUtility)
 * AUTHOR: George Karaszi 10-4-2013
 * FILE ACCESSED:
 *          (INPUT) MainData.bin
 *          (INPUT) MainDataCollisions.bin
 *          (OUTPUT) log.txt
 *          
 * USEAGE: To access and showcase the data stored within binaray files. 
 *         Quick run of the mill looping structure that will loops till 
 *         the end of the file, collecting information and displaying it 
 *         in a formatted matter.
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
        static int _sizeOfDataRec = 3 + 17 + 12 + sizeof(int) +
                                    sizeof(short) + sizeof(long) + 
                                    sizeof(float) + sizeof(int) + 
                                    sizeof(short);

        static short nCol; //Number of collisions.
        static short nRec; //Number of records.


        public static void Main(string[] args)
        {

            fMainDataFile = new FileStream("MainData.bin", FileMode.Open);
            bMainDataFileReader = new BinaryReader(fMainDataFile);
            nRec = (short)(bMainDataFileReader.ReadInt16() - 1);
            nCol = (short)(bMainDataFileReader.ReadInt16() - 1);


            fCollisionDataFile = new FileStream("MainDataCollision.bin", FileMode.Open);
            bCollisionDataFileReader = new BinaryReader(fCollisionDataFile);

            string[] MainRecordList      = ReadFile(bMainDataFileReader, _sizeOfHeaderRec);
            string[] CollisionRecordList = ReadFile(bCollisionDataFileReader,0);

            PrintResults(MainRecordList, CollisionRecordList);
            FinishUp();
          
        }

        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Loop through and store all viable records into a array
        /// </summary>
        /// <param name="fileReader">Current binary reader for the file being accessed</param>
        /// <param name="headerLength">Length (in bytes) of the header file</param>
        /// <returns>An array of formatted records ready to be displayed</returns>

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
            short link;
            int RRN = 1;
            string formatRecord;
            List<string> RecordCollection = new List<string>(); //List of formatted record strings

            for (long pos = headerLength; pos < fileReader.BaseStream.Length; pos += _sizeOfDataRec, RRN++)
            {
                fileReader.BaseStream.Seek(pos, SeekOrigin.Begin);

                code = fileReader.ReadChars(3);

                if (code[0] == '\0')
                {
                    formatRecord = "[" + Convert.ToString(RRN).PadLeft(3, '0') + "]".PadRight(2) + "Empty";
                }
                else
                {

                    name = fileReader.ReadChars(17);
                    continent = fileReader.ReadChars(12);
                    surfaceArea = fileReader.ReadInt32();
                    yearOfIndep = fileReader.ReadInt16();
                    population = fileReader.ReadInt64();
                    lifeExpectancy = fileReader.ReadSingle();
                    gnp = fileReader.ReadInt32();
                    link = fileReader.ReadInt16();

               
                    formatRecord =  "[" + Convert.ToString(RRN).PadLeft(3, '0') + "]".PadRight(2) +
                                    new string(code).PadRight(6) +
                                    new string(name).PadRight(18) +
                                    new string(continent).PadRight(12) +
                                    string.Format("{0:#,###,###.##}", surfaceArea).PadLeft(10) +
                                    Convert.ToString(yearOfIndep).PadLeft(6).PadRight(7) +
                                    string.Format("{0:#,###,###,###}", population).PadLeft(13).PadRight(12) +
                                    string.Format("{0:0.0#}", lifeExpectancy).PadRight(1).PadLeft(5) +
                                    string.Format("{0:#,###,###,###}", gnp).PadLeft(10) +
                                    Convert.ToString(link).PadLeft(5);
                }

                RecordCollection.Add(formatRecord);
            }

            return RecordCollection.ToArray();

        }


        //-----------------------------------------------------------------------------
        /// <summary>
        /// Close all files that are open
        /// </summary>
        private static void FinishUp()
        {
            bMainDataFileReader.Close();
            fMainDataFile.Close();

            bCollisionDataFileReader.Close();
            fCollisionDataFile.Close();
        }



        //------------------------------------------------------------------------------
        /// <summary>
        /// Formates the header to be displayed
        /// </summary>
        /// <returns>A ready to use string aligned in its columns</returns>
        private static string FormatHeader()
        {

            return "[RRN]".PadRight(6) + 
                   "CODE".PadRight(6) + 
                   "NAME".PadRight(18, '-') + 
                   "CONTINENT".PadRight(12, '-') +
                   "AREA".PadLeft(7, '-').PadRight(10, '-') + 
                   "INDEP".PadRight(6).PadLeft(7) + 
                   "POPULATION".PadLeft(12, '-').PadRight(13, '-') + 
                   "L.EX".PadRight(5).PadLeft(6) + 
                   "GNP".PadLeft(6, '-').PadRight(9, '-') + 
                   "LINK".PadLeft(5);   
        }

        //------------------------------------------------------------------------------
        /// <summary>
        /// Print the results from the formatted text
        /// </summary>
        private static void PrintResults(string[] MainDataList, string []CollisionDataList)
        {
            StreamWriter logFile = new StreamWriter("Log.txt", true);
            string header = FormatHeader();

            logFile.WriteLine("\n***************Pretty Print Start***************\n");
            logFile.WriteLine("MAIN DATA - HOME AREA".PadRight(header.Length + 1, '*'));
            logFile.WriteLine(header);
       
            foreach (string s in MainDataList)
            {
                logFile.WriteLine(s);
            }

            logFile.WriteLine("End".PadRight(header.Length + 1, '*'));
            logFile.WriteLine();
            logFile.WriteLine();
            logFile.WriteLine("MAIN DATA - COLLISION AREA".PadRight(header.Length + 1, '*'));
            logFile.WriteLine(FormatHeader());

            foreach(string s in CollisionDataList)
            {
                logFile.WriteLine(s);
            }
            logFile.WriteLine("End".PadRight(header.Length + 1, '*'));
            logFile.WriteLine();
            logFile.WriteLine();
            logFile.WriteLine("#Rec in Home Area: {0},   #Rec in Collision Area: {1}", nRec, nCol);
            logFile.WriteLine("\n**********End Of Pretty Print Utility**********\n");

            logFile.Close();
        }

    }
}
