/* PROJECT:  Asign 1 (C#)            PROGRAM: MainData class
 * AUTHOR: George Karaszi   
 *******************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace SharedClassLibrary
{
    public class MainData
    {
        //**************************** PRIVATE DECLARATIONS ************************

        private FileStream fMainDataFile;                  //RandomAccess File structure
        private FileStream fCollisionDataFile;
        private BinaryWriter bMDataFileWriter;
        private BinaryReader bMDataFileReader;
        private BinaryWriter bCollisionDataFileWriter;
        private BinaryReader bCollisionDataFileReader;

        private UserInterface _LogFile;                  //Log file Access
        private string mDataFileName;                        //Holds the file name of main data
        private string cDataFileName;

        private short nHomeRec = 1;                    //Counts how many recorders
        private short nCollRec = 1;
        private short MAX_N_HOME_LOC;
        private char[] collisionFileName;

        private int _sizeOfHeaderRec;                   //Size of the reader record
        private int _sizeOfDataRec;                     //Size of all the data fields
        private char[] _code = new char[3];             //Country code
        private char[] _name = new char[17];            //Name of country
        private char[] _continent = new char[12];       //What continent the country is located
        private int  _surfaceArea;                      //Size of the country
        private short _yearOfIndep;                     //What year they went independent
        private long _population;                       //Total population of the country
        private float _lifeExpectancy;                  //The average time someone is alive in the country
        private int _gnp;                                //Gross national product

        //**************************** PUBLIC GET/SET METHODS **********************


        //**************************** PUBLIC CONSTRUCTOR(S) ***********************
        public MainData(UserInterface LogInterFace)
        {
            //Calculate sizes for RandomAccess byte offset
            _sizeOfHeaderRec = sizeof(short);
            _sizeOfDataRec   = + _code.Length + _name.Length + _continent.Length
                               + sizeof(int) +sizeof(short) + sizeof(long) + 
                                 sizeof(float) + sizeof(int) + sizeof(short);


            //Open and create a new file
            mDataFileName = "MainData.txt";
            cDataFileName = "MainDataCollision.bin";
            collisionFileName = cDataFileName.ToCharArray();

            //Allow access to log file
            _LogFile = LogInterFace;

            //Open or Create Main data file
            fMainDataFile = new FileStream(mDataFileName, FileMode.OpenOrCreate);
            bMDataFileReader = new BinaryReader(fMainDataFile);
            bMDataFileWriter = new BinaryWriter(fMainDataFile);

            fCollisionDataFile = new FileStream(cDataFileName, FileMode.OpenOrCreate);
            bCollisionDataFileWriter = new BinaryWriter(fCollisionDataFile);
            bCollisionDataFileReader = new BinaryReader(fCollisionDataFile);
            
         
            _LogFile.WriteToLog("Opened " + mDataFileName + " File");
            _LogFile.WriteToLog("Opened " + cDataFileName + " File");
            MAX_N_HOME_LOC = 20;

            //Get total records in file (Default is 0)
            nHomeRec = ReadHeaderRecCount();
        }

        //**************************** PUBLIC SERVICE METHODS **********************

        /// <summary>
        /// Stores one country to the main data file
        /// </summary>
        /// <param name="RD">Raw data class that holds parsed values</param>
        /// <returns>The boolean value of whether the main data file has a dup</returns>
        public bool StoreOneCountry(RawData RD)
        {

            InitializeCustomVaraibles(RD);

            int RRN = HashFunction(_code);
            int byteOffSet = CalculateByteOffSet(RRN);

            if(RecordIsFilled(byteOffSet))
            {
                CollisionHandling(byteOffSet);
                ++nCollRec;
            }


           
            WriteOneCountry(byteOffSet);
            ++nHomeRec; //increase amount of records

            return true;

        }



        //-------------------------------------------------------------------------
        /// <summary>
        /// Closes the main data file 
        /// </summary>
        public void FinishUp()
        {
            WriteHeaderRec();
            bMDataFileReader.Close();
            bMDataFileWriter.Close();
            fMainDataFile.Close();

            bCollisionDataFileWriter.Close();
            fCollisionDataFile.Close();


            _LogFile.WriteToLog("Closed " + mDataFileName + " File");
            _LogFile.WriteToLog("Closed " + cDataFileName + " File");

        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Returns a recorded by ID
        /// </summary>
        /// <param name="id">ID of recorded requested</param>
        public void QueryByID(string queryID)
        {
            _LogFile.WriteToLog("**Error: Sorry Query By ID is no longer working");
        }

        //-------------------------------------------------------------------------------
        /// <summary>
        /// Gives a list of all recorders by ID
        /// </summary>
        /// <param name="queryIDList">A list of ID's that need to be queried</param>
        /// <returns>An Array of non-error records</returns>
        public void ListById()
        {
            _LogFile.WriteToLog("**Error: Sorry Listing records no longer works");
        }


        //---------------------------------------------------------------------------
        /// <summary>
        /// Erases a recorded by places a terminating character on the records place.
        /// </summary>
        /// <param name="id">Record id</param>
        public void DeleteRecordByID(string queryID)
        {
            _LogFile.WriteToLog("**Error: Sorry Deleting recorded is no longer working");
        }


        //-------------------------------------------------------------------------
        /// <summary>
        /// Inserts a new record into the file
        /// </summary>
        /// <param name="record">A string with CSV style record</param>
        public void InsertRecord(string record)
        {
            _LogFile.WriteToLog("*IN: Is not operational at this time");

        }

        //**************************** PRIVATE METHODS *****************************

        //---------------------------------------------------------------------------
        /// <summary>
        /// Obtain the offset to where the file pointer needs to point
        /// </summary>
        /// <param name="RRN">An ID to what record that needs to be obtained</param>
        /// <returns>offset to file positions</returns>
        private int CalculateByteOffSet(int RRN)
        {
            return _sizeOfHeaderRec + ((RRN - 1) * _sizeOfDataRec);
        }

        //--------------------------------------------------------------------------
        /// <summary>
        /// Gives the RRN to where the information needs to be stored to stored or 
        /// queried
        /// </summary>
        /// <param name="id">Id to calculate the RRN of the document</param>
        /// <returns>The RRN of the main data file</returns>
        private int HashFunction(char []Code)
        {
            int RRN = 0;
            byte[] asciiBytes = Encoding.ASCII.GetBytes(new string(Code).ToUpper());
            RRN = ((asciiBytes[0] * asciiBytes[1] * asciiBytes[2]) % MAX_N_HOME_LOC) + 1;


            return RRN;
        }

        //--------------------------------------------------------------------------
        /// <summary>
        /// Checks the record to see if something is in its place already
        /// </summary>
        /// <param name="byteOffSet">Where in the file it needs to look at</param>
        /// <returns>If true file is already filled</returns>
        private bool RecordIsFilled(int byteOffSet)
        {
            byte data;

            fMainDataFile.Seek(byteOffSet, SeekOrigin.Begin);
            data = bMDataFileReader.ReadByte();

            if(data != '\0')  //If there is already data there in the file, a duplicate was found
            {

                return true;
            }
            
            return false;
        }



        //------------------------------------------------------------------------------
        /// <summary>
        /// Reads the header that contains the amount of records inside
        /// </summary>
        /// <returns>Record amount number</returns>

        private short ReadHeaderRecCount()
        {
            fMainDataFile.Seek(0, SeekOrigin.Begin);
            return bMDataFileReader.ReadInt16();
        }

        //----------------------------------------------------------------------------------
        /// <summary>
        /// Writes the header record to the top of the file with current record count
        /// </summary>
        private void WriteHeaderRec()
        {
            fMainDataFile.Seek(0, SeekOrigin.Begin);
            bMDataFileWriter.Write(nHomeRec);
        }

        //------------------------------------------------------------------------------
        /// <summary>
        /// Formates the string to de aligned with its header columns
        /// </summary>
        /// <param name="record">record from main data</param>
        /// <returns>formatted string ready to be used</returns>
        private string FormatRecord(string record)
        {
            int stringPos = 0;

            string id = record.Substring(stringPos, 3).Trim();
            stringPos += 3;
            string code = record.Substring(stringPos, 3).Trim();
            stringPos += 3;
            string name = record.Substring(stringPos, 17).Trim();
            stringPos += 17;
            string continent = record.Substring(stringPos, 11).Trim();
            stringPos += 11;
            string region = record.Substring(stringPos, 10).Trim();
            stringPos += 10;
            string surfaceArea = record.Substring(stringPos, 8).Trim();
            stringPos += 8;
            string yearOfIndep = record.Substring(stringPos, 5).Trim();
            stringPos += 5;
            string population = record.Substring(stringPos, 10).Trim();
            stringPos += 10;
            string lifeExpectancy = record.Substring(stringPos, 4).Trim();




            string t =  id.PadRight(4, ' ') +
                        code.PadRight(5, ' ') +
                        name.PadRight(20, ' ') +
                        continent.PadRight(18) +
                        region.PadRight(15, ' ') +
                        surfaceArea.PadRight(15, ' ') +
                        yearOfIndep.PadRight(9, ' ') +
                        population.PadRight(13, ' ') +
                        lifeExpectancy;

            return t;
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Returns a header that is formated to show all data that is inputted
        /// </summary>
        /// <returns>Header string</returns>

        private string FormatHeader()
        {

            string t =  "ID".PadRight(4, ' ') +
                        "CODE".PadRight(5, ' ') +
                        "NAME".PadRight(20, ' ') +
                        "CONTINENT".PadRight(18, ' ') +
                        "REGION".PadRight(15, ' ') +
                        "AREA".PadRight(15, ' ') +
                        "INDEP".PadRight(9, ' ') +
                        "POPULATION".PadRight(13, ' ') +
                        "L.EXP";

            return t;
        }

        //---------------------------------------------------------------------------
        /// <summary>
        /// Reads the top of the record file
        /// </summary>
        /// <returns>Returns the amount of records stored in file</returns>
        private int ReadRecordCount()
        {
            fMainDataFile.Seek(0, SeekOrigin.Begin);
            return  bMDataFileReader.ReadInt32();
        }

        //--------------------------------------------------------------------------
        /// <summary>
        /// Reads one block of data from the file based on the RRN
        /// </summary>
        /// <param name="RRN">Record location</param>
        /// <returns>A string based on its RRN location in file</returns>
        private byte []ReadOneRecord(int RRN)
        {
            int byteOffSet    = CalculateByteOffSet(RRN);

            fMainDataFile.Seek(byteOffSet, SeekOrigin.Begin);

            return ReadOneRecord();
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Reads one record at its current position in the file stream.
        /// </summary>
        /// <returns>Array of the record</returns>
        private byte[] ReadOneRecord()
        {
            byte[] recordData = new byte[_sizeOfDataRec];
            bMDataFileReader.Read(recordData, 0, recordData.Length);

            return recordData;
        }

        //--------------------------------------------------------------------------
        /// <summary>
        /// Writes one country to the file by the given byteOffSet
        /// </summary>
        /// <param name="byteOffSet">Where in the file to begin the writing process</param>
        private void WriteOneCountry(int byteOffSet)
        {

            //Move file pointer to new location
            fMainDataFile.Seek(byteOffSet, SeekOrigin.Begin);

            //Write the information to the maindata file;
            WriteRecord(bMDataFileWriter);

        }


        //-----------------------------------------------------------------------------
        /// <summary>
        /// Initializes all the variables that require a fixed length
        /// </summary>
        /// <param name="RD">Class that holds strings at random lengths</param>
        private void InitializeCustomVaraibles(RawData RD)
        {
            _code        = RD.CODE.PadLeft(_code.Length, ' ').ToCharArray(0, _code.Length);
            _name        = RD.NAME.PadRight(_name.Length, ' ').ToCharArray(0, _name.Length);
            _continent   = RD.CONTINENT.PadRight(_continent.Length, ' ').ToCharArray(0, _continent.Length);
            _surfaceArea = Int32.Parse(RD.SURFACEAREA);


            if(RD.YEAROFINDEP.Contains("-"))
            {
                RD.YEAROFINDEP = RD.YEAROFINDEP.Replace('-', '0');
            }

            _yearOfIndep = Int16.Parse(RD.YEAROFINDEP);
            _population  = long.Parse(RD.POPULATION);

            //Check this (needs to be XX.X or X.XX or null)
            if (RD.LIFEEXPECTANCY.ToUpper().CompareTo("NULL") == 0)
            {
                _lifeExpectancy = 0.0f;
            }
            else
            {
                _lifeExpectancy = float.Parse(RD.LIFEEXPECTANCY);
            }

            _gnp            = Int32.Parse(RD.GNP);


        }

        //-----------------------------------------------------------------------
        /// <summary>
        /// Handles the collision data from main data file. And places new 
        /// information in the collision file.
        /// </summary>
        /// <param name="byteOffSet"></param>
        private int CollisionHandling(int MainDatabyteOffSet)
        {

            int collisionByteOffSet = CalculateByteOffSet(nCollRec);
            byte record;
            fCollisionDataFile.Seek(collisionByteOffSet, SeekOrigin.Begin);
            record = bCollisionDataFileReader.ReadByte();

            //Find empty RRN location
            while(record != '\0')
            {
                collisionByteOffSet = CalculateByteOffSet(++nCollRec);
                fCollisionDataFile.Seek(collisionByteOffSet, SeekOrigin.Begin);
                record = bCollisionDataFileReader.ReadByte();
            }

            fCollisionDataFile.Seek(-1, SeekOrigin.Current);

            WriteRecord(bCollisionDataFileWriter);

            return ChangeMainDataHeaderPtr(MainDatabyteOffSet);
            
        }

        //-----------------------------------------------------------------------
        /// <summary>
        /// Changes the header pointer in the data field, if data field is empty
        /// </summary>
        /// <param name="byteOffSet">Location of data</param>
        /// <returns>Current pointed header record</returns>
        private int ChangeMainDataHeaderPtr(int MainDatabyteOffSet)
        {
            int headerPtr = 0;

            fMainDataFile.Seek(MainDatabyteOffSet + (_sizeOfDataRec - sizeof(short)), 
                               SeekOrigin.Begin);

            headerPtr = bMDataFileReader.ReadInt16();

            //If header ptr is empty, write new location
            if(headerPtr == -1)
            {
                fMainDataFile.Seek(MainDatabyteOffSet + (_sizeOfDataRec - sizeof(short)),
                                   SeekOrigin.Begin);

                bMDataFileWriter.Write(nCollRec);
                return nCollRec;
            }

            return headerPtr;

        }

        //------------------------------------------------------------------------
        /// <summary>
        /// Writes All data to a file based on its Writer function in use
        /// </summary>
        /// <param name="bw">What file structure is being written to</param>
        private void WriteRecord(BinaryWriter bw)
        {
            bw.Write(_code);
            bw.Write(_name);
            bw.Write(_continent);
            bw.Write(_surfaceArea);
            bw.Write(_yearOfIndep);
            bw.Write(_population);
            bw.Write(_lifeExpectancy);
            bw.Write(_gnp);
            bw.Write(-1);
        }
    }
}
