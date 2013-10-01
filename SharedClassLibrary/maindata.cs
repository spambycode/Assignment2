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

        private FileStream mainDataFile;                //RandomAccess File structure
        private UserInterface _LogFile;                  //Log file Access
        private string fileName;                        //Holds the file name of main data
        private int headerCount = 0;                    //Counts how many recorders
        private int _sizeOfHeaderRec;                   //Size of the reader record
        private int _sizeOfDataRec;                     //Size of all the data fields
        private char[] _headerRec = new char[3];        //Header rec of the document
        private char[] _id = new char[3];               //ID of record
        private char[] _code = new char[3];             //Country code
        private char[] _name = new char[17];            //Name of country
        private char[] _continent = new char[11];       //What continent the country is located
        private char[] _region = new char[10];          //What region the country is located
        private char[] _surfaceArea = new char[8];      //Size of the country
        private char[] _yearOfIndep = new char[5];      //What year they went independent
        private char[] _population = new char[10];      //Total population of the country
        private char[] _lifeExpectancy = new char[4];   //The average time someone is alive in the country

        //**************************** PUBLIC GET/SET METHODS **********************


        //**************************** PUBLIC CONSTRUCTOR(S) ***********************
        public MainData(UserInterface LogInterFace)
        {
            //Calculate sizes for RandomAccess byte offset
            _sizeOfHeaderRec = _headerRec.Length;
            _sizeOfDataRec   = _id.Length + _code.Length + _name.Length + _continent.Length
                               + _region.Length + _surfaceArea.Length + _yearOfIndep.Length
                               + _population.Length + _lifeExpectancy.Length;


            //Open and create a new file
            fileName = "MainData.txt";

            //Allow access to log file
            _LogFile = LogInterFace;

            //Open or Create Main data file
            mainDataFile = new FileStream(fileName, FileMode.OpenOrCreate);
            _LogFile.WriteToLog("Opened " + fileName + " File");

            //Get total records in file (Default is 0)
            headerCount = ReadHeaderRec();
        }

        //**************************** PUBLIC SERVICE METHODS **********************

        /// <summary>
        /// Stores one country to the main data file
        /// </summary>
        /// <param name="RD">Raw data class that holds parsed values</param>
        /// <returns>The boolean value of whether the main data file has a dup</returns>
        public bool StoreOneCountry(RawData RD)
        {

            InitializeFixLengthVaraibles(RD);

            int RRN = CalculateRRN(RD.ID);
            int byteOffSet = CalculateByteOffSet(RRN);

            if(RecordIsFilled(byteOffSet))
            {
                return false;
            }


            ++headerCount; //increase amount of records
            WriteOneCountry(byteOffSet);

            return true;

        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Closes the main data file 
        /// </summary>
        public void FinishUp()
        {
            mainDataFile.Close();
            _LogFile.WriteToLog("Closed " + fileName + " File");
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Returns a recorded by ID
        /// </summary>
        /// <param name="id">ID of recorded requested</param>
        public void QueryByID(string queryID)
        {
            string recordReturn = "";
            int ID = Convert.ToInt32(queryID.Trim());
            int RRN = 0;
            byte[] record = { 0 };

            if (ID > 0 && ID <= 300)
            {
                RRN = CalculateRRN(ID);
                record = ReadOneRecord(RRN);

                if (record[0] != '\0')
                {
                    recordReturn = FormatRecord(Encoding.UTF8.GetString(record));
                }
                else
                {
                    recordReturn = string.Format("**ERROR: no country with id {0}", queryID);
                }
            }
            else
            {
                recordReturn = string.Format("**ERROR: no country with id {0}", queryID);
            }

            _LogFile.WriteToLog(recordReturn);
        }

        //-------------------------------------------------------------------------------
        /// <summary>
        /// Gives a list of all recorders by ID
        /// </summary>
        /// <param name="queryIDList">A list of ID's that need to be queried</param>
        /// <returns>An Array of non-error records</returns>
        public void ListById()
        {
            byte[] QueryRecord;                //Record that was returned
            int id = 1;
            int RRN = 0;

            _LogFile.WriteToLog(FormatHeader());

            for(int i = 0; i < headerCount;)
            {
                RRN = CalculateRRN(id++);
                QueryRecord = ReadOneRecord(RRN);

                if(QueryRecord[0] != 0)
                {
                    _LogFile.WriteToLog(FormatRecord(Encoding.UTF8.GetString(QueryRecord)));
                    ++i;
                }

            }



        }


        //---------------------------------------------------------------------------
        /// <summary>
        /// Erases a recorded by places a terminating character on the records place.
        /// </summary>
        /// <param name="id">Record id</param>
        public void DeleteRecordByID(string queryID)
        {
            byte[] data = new byte[_sizeOfDataRec];
            int ID = Convert.ToInt32(queryID);


            if (ID > 0 && ID <= 900)
            {

                int byteOffSet = CalculateByteOffSet(CalculateRRN(ID));


                mainDataFile.Seek(byteOffSet, SeekOrigin.Begin);
                mainDataFile.Read(data, 0, data.Length);

                if (data[0] != '\0')
                {
                    string name = Encoding.UTF8.GetString(data)
                                            .Substring(_id.Length + _code.Length, _name.Length).Trim();

                    //Apply null byte to record
                    mainDataFile.Seek(byteOffSet, SeekOrigin.Begin);
                    mainDataFile.WriteByte(0);

                    //Subtract record count and write new header
                    --headerCount;
                    WriteHeaderRec();

                    _LogFile.WriteToLog("OK, " + name + "\t\tDeleted");
                }
                else
                {
                    _LogFile.WriteToLog("Sorry no country with id " + queryID);
                }
            }
            else
            {
                _LogFile.WriteToLog("Sorry no country with id " + queryID);
            }
        }


        //-------------------------------------------------------------------------
        /// <summary>
        /// Inserts a new record into the file
        /// </summary>
        /// <param name="record">A string with CSV style record</param>
        public void InsertRecord(string record)
        {

            /*var recordSplit = record.Split(',');

            //Must of at least 9 records for everything to work
            if(recordSplit.Length >= 9)
            {
                RawData RD = new RawData(recordSplit);
                StoreOneCountry(RD);
                _LogFile.WriteToLog("IN : OK, " + recordSplit[2] + " Has been added to MainData");
            }
            else
            {
                _LogFile.WriteToLog("IN : Wrong input was found");
            }*/

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


        //-------------------------------------------------------------------------
        /// <summary>
        /// Returns the RRN value of the given parameter
        /// </summary>
        /// <param name="id">ID of query record</param>
        /// <returns></returns>
        private int CalculateRRN(int id)
        {
            return id;
        }

        //--------------------------------------------------------------------------
        /// <summary>
        /// Gives the RRN to where the information needs to be stored to stored or 
        /// queried
        /// </summary>
        /// <param name="id">Id to calculate the RRN of the document</param>
        /// <returns>The RRN of the main data file</returns>
        private int CalculateRRN(string id)
        {
            return CalculateRRN(Int32.Parse(id));
        }

        //--------------------------------------------------------------------------
        /// <summary>
        /// Checks the record to see if something is in its place already
        /// </summary>
        /// <param name="byteOffSet">Where in the file it needs to look at</param>
        /// <returns>If true file is already filled</returns>
        private bool RecordIsFilled(int byteOffSet)
        {
            byte[] Data = new Byte[_sizeOfDataRec];   //Data being read from file
            string inFileName = "";
            string inFileId     = "";

            mainDataFile.Seek(byteOffSet, SeekOrigin.Begin);
            mainDataFile.Read(Data, 0, Data.Length);

            if(Data[0] != '\0')  //If there is already data there in the file, a duplicate was found
            {

                //Get the name and ID of the information found in the file
                inFileName = Encoding.UTF8.GetString(Data)
                                          .Substring(_id.Length + _code.Length, _name.Length).Trim();

                inFileId = Encoding.UTF8.GetString(Data)
                                        .Substring(0, _id.Length).Trim();

                //Write a nice little error log msg to the file.
                _LogFile.WriteToLog("**Error: ID "  + new string(_id) + 
                                        " Not inserted (ID " + inFileId + " belongs to " + inFileName + ")");

                return true;
            }
            

            return false;
        }



        //------------------------------------------------------------------------------
        /// <summary>
        /// Reads the header that contains the amount of records inside
        /// </summary>
        /// <returns>Record amount number</returns>

        private int ReadHeaderRec()
        {
            byte[] recordData = new byte[_sizeOfHeaderRec];

            mainDataFile.Seek(0, SeekOrigin.Begin);
            mainDataFile.Read(recordData, 0, recordData.Length);


            if(recordData[0] != 0)
            {
                return Convert.ToInt32(Encoding.UTF8.GetString(recordData));;
            }


            return 0;
        }

        //----------------------------------------------------------------------------------
        /// <summary>
        /// Writes the header record to the top of the file with current record count
        /// </summary>
        private void WriteHeaderRec()
        {
            mainDataFile.Seek(0, SeekOrigin.Begin);
            _headerRec = headerCount.ToString().PadLeft(_headerRec.Length, '0').ToCharArray();
            WriteOneRecord(_headerRec);
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
            byte[] header = new byte[3];

            mainDataFile.Seek(0, SeekOrigin.Begin);
            mainDataFile.Read(header, 0, header.Length);

            return Int32.Parse(Encoding.UTF8.GetString(header));

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

            mainDataFile.Seek(byteOffSet, SeekOrigin.Begin);

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

            mainDataFile.Read(recordData, 0, recordData.Length);

            return recordData;
        }

        //--------------------------------------------------------------------------
        /// <summary>
        /// Writes one country to the file by the given byteOffSet
        /// </summary>
        /// <param name="byteOffSet">Where in the file to begin the writing process</param>
        private void WriteOneCountry(int byteOffSet)
        {
            WriteHeaderRec();

            if(mainDataFile.Length < byteOffSet)
                mainDataFile.SetLength(byteOffSet);

            //Move file pointer to new location
            mainDataFile.Seek(byteOffSet, SeekOrigin.Begin);

            //Write the information to the maindata file
            WriteOneRecord(_id);
            WriteOneRecord(_code);
            WriteOneRecord(_name);
            WriteOneRecord(_continent);
            WriteOneRecord(_region);
            WriteOneRecord(_surfaceArea);
            WriteOneRecord(_yearOfIndep);
            WriteOneRecord(_population);
            WriteOneRecord(_lifeExpectancy);

        }


        //-----------------------------------------------------------------------------
        /// <summary>
        /// Initializes all the variables that require a fixed length
        /// </summary>
        /// <param name="RD">Class that holds strings at random lengths</param>
        private void InitializeFixLengthVaraibles(RawData RD)
        {
            string lifeExp = "";

            _id          = RD.ID.PadLeft(_id.Length, '0').ToCharArray(0, _id.Length);
            _code        = RD.CODE.PadLeft(_code.Length, ' ').ToCharArray(0, _code.Length);
            _name        = RD.NAME.PadRight(_name.Length, ' ').ToCharArray(0, _name.Length);
            _continent   = RD.CONTINENT.PadRight(_continent.Length, ' ').ToCharArray(0, _continent.Length);
            _region      = RD.REGION.PadRight(_region.Length, ' ').ToCharArray(0, _region.Length);
            _surfaceArea = RD.SURFACEAREA.PadLeft(_surfaceArea.Length, '0').ToCharArray(0, _surfaceArea.Length);


            if(RD.YEAROFINDEP.Contains("-"))
            {
                RD.YEAROFINDEP = RD.YEAROFINDEP.Replace('-', '0');
            }

            _yearOfIndep = RD.YEAROFINDEP.PadLeft(_yearOfIndep.Length, '0').ToCharArray(0, _yearOfIndep.Length);
            _population  = RD.POPULATION.PadLeft(_population.Length, '0').ToCharArray(0, _population.Length);

            //Check this (needs to be XX.X or X.XX or null)
            if(RD.LIFEEXPECTANCY.ToUpper().CompareTo("NULL") == 0)
            {
                lifeExp = RD.LIFEEXPECTANCY;
            }
            else
            {
                lifeExp = string.Format("{0:N1}", Convert.ToDecimal(RD.LIFEEXPECTANCY));
            }

            _lifeExpectancy = lifeExp.ToCharArray();


        }

        //-------------------------------------------------------------------------------
        /// <summary>
        /// Uses the write function in file stream to write a record to the main file
        /// </summary>
        /// <param name="input">Input wanted to write to file</param>

        private void WriteOneRecord(char[] input)
        {
            mainDataFile.Write(Encoding.ASCII.GetBytes(input), 0, input.Length);
        }

    }
}
