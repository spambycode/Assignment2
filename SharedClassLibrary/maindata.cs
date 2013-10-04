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
        private short _link;                            //Link to collision data

        //**************************** PUBLIC GET/SET METHODS **********************


        //**************************** PUBLIC CONSTRUCTOR(S) ***********************
        public MainData(UserInterface LogInterFace)
        {
            //Calculate sizes for RandomAccess byte offset
            _sizeOfHeaderRec = sizeof(short) + sizeof(short);
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

            if (RecordIsFilled(byteOffSet))
            {
                CollisionHandling(byteOffSet);
                ++nCollRec;
            }
            else
            {
                WriteOneCountry(byteOffSet);
                ++nHomeRec; //increase amount of records in main data
            }

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


        //-------------------------------------------------------------------------
        /// <summary>
        /// Finds the code related to the input.
        /// </summary>
        /// <param name="queryCode">Code that is stored in the file</param>
        public void QueryByCode(string queryCode)
        {
            int recordsChecked = 1;
            string recordReturn = "";
            char[] Code = queryCode.Trim().ToCharArray();

            ReadOneRecord(HashFunction(Code));

            if (_code[0] != '\0')
            {
                if (queryCode.ToUpper().CompareTo(new string(_code).ToUpper()) == 0)
                {
                    recordReturn = FormatRecord();
                }
                else 
                {
                    SearchCollisionFile(queryCode, ref recordsChecked);

                    if (SearchCollisionFile(queryCode, ref recordsChecked) != null)
                    {
                        recordReturn = FormatRecord();
                    }
                    else
                    {
                        recordReturn = string.Format("**ERROR(QUERY): no country with code {0}", queryCode.Trim());
                    }

                }

            }
            else
            {
                recordReturn = string.Format("**ERROR(QUERY): no country with code {0}", queryCode.Trim());
            }
            

            _LogFile.WriteToLog(recordReturn);
            _LogFile.WriteToLog("[" + Convert.ToString(recordsChecked) + "]");
        }

        //-------------------------------------------------------------------------
        /// <summary>
        /// Delete a record by its code
        /// </summary>
        /// <param name="queryCode">Country code</param>
        public void DeleteByCode(string queryCode)
        {
            int recordsChecked = 1;
            string recordReturn = "";
            char[] Code = queryCode.Trim().ToCharArray();

            ReadOneRecord(HashFunction(Code));

            if (_code[0] != '\0')
            {
                if (queryCode.ToUpper().CompareTo(new string(_code).ToUpper()) == 0)
                {
                    //found match in maindata
                }
                else
                {
                    int []byteoffSet = SearchCollisionFile(queryCode, ref recordsChecked);

                    if(byteoffSet.Length > 0)
                    {
                        TomeStoneRecord(byteoffSet, recordsChecked, Code);  
                    }
                    else
                    {
                        recordReturn = string.Format("**ERROR(DELETE): no country with code {0}", queryCode.Trim());
                    }
                }
            }
            else
            {
                recordReturn = string.Format("**ERROR(DELETE): no country with code {0}", queryCode.Trim());
            }

            _LogFile.WriteToLog(recordReturn);
            _LogFile.WriteToLog("[" + Convert.ToString(recordsChecked) + "]");
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
            bMDataFileWriter.Write(nCollRec);
        }

        //------------------------------------------------------------------------------
        /// <summary>
        /// Formates the string to de aligned with its header columns
        /// </summary>
        /// <param name="record">record from main data</param>
        /// <returns>formatted string ready to be used</returns>
        private string FormatRecord()
        {
            string code = new string(_code);
            string name = new string(_name);
            string continent = new string(_continent);
            string surfaceArea = Convert.ToString(_surfaceArea);
            string yearOfIndep = Convert.ToString(_yearOfIndep);
            string population = Convert.ToString(_population);
            string lifeExpectancy = Convert.ToString(_lifeExpectancy);
            string gnp = Convert.ToString(_gnp);




            string t =  code.PadRight(5, ' ') +
                        name.PadRight(20, ' ') +
                        continent.PadRight(18) +
                        surfaceArea.PadRight(15, ' ') +
                        yearOfIndep.PadRight(9, ' ') +
                        population.PadRight(13, ' ') +
                        lifeExpectancy.PadRight(8, ' ') +
                        gnp;

            return t;
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Returns a header that is formated to show all data that is inputted
        /// </summary>
        /// <returns>Header string</returns>

        private string FormatHeader()
        {

            string t =  "CODE".PadRight(5, ' ') +
                        "NAME".PadRight(20, ' ') +
                        "CONTINENT".PadRight(18, ' ') +
                        "AREA".PadRight(15, ' ') +
                        "INDEP".PadRight(9, ' ') +
                        "POPULATION".PadRight(13, ' ') +
                        "L.EXP".PadRight(8, ' ')+ 
                        "GNP";

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
        private void ReadOneRecord(int RRN)
        {
            int byteOffSet    = CalculateByteOffSet(RRN);
            fMainDataFile.Seek(byteOffSet, SeekOrigin.Begin);

            ReadOneRecord();
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Reads one record at its current position in the file stream.
        /// </summary>
        private void ReadOneRecord()
        {
            _code        = bMDataFileReader.ReadChars(_code.Length);
            _name        = bMDataFileReader.ReadChars(_name.Length);
            _continent   = bMDataFileReader.ReadChars(_continent.Length);
            _surfaceArea = bMDataFileReader.ReadInt32();
            _yearOfIndep = bMDataFileReader.ReadInt16();
            _population  = bMDataFileReader.ReadInt64();
            _lifeExpectancy = bMDataFileReader.ReadSingle();
            _gnp         = bMDataFileReader.ReadInt32();
            _link        = bMDataFileReader.ReadInt16();
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Reads a record stored in the collision file
        /// </summary>
        /// <param name="RRN">Place where record is kept</param>
        /// <returns>Returns the byteoffset that was read</returns>
        private int ReadOneCollisionRecord(int RRN)
        {
            int byteOffSet = CalculateByteOffSet(_link) - _sizeOfHeaderRec;
            fCollisionDataFile.Seek(byteOffSet, SeekOrigin.Begin);
            ReadOneCollisionRecord();

            return byteOffSet;
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Reads a record from the collision file
        /// </summary>
        private void ReadOneCollisionRecord()
        {
            long currentPosition = fCollisionDataFile.Position;
            _code        = bCollisionDataFileReader.ReadChars(_code.Length);
            _name        = bCollisionDataFileReader.ReadChars(_name.Length);
            _continent   = bCollisionDataFileReader.ReadChars(_continent.Length);
            _surfaceArea = bCollisionDataFileReader.ReadInt32();
            _yearOfIndep = bCollisionDataFileReader.ReadInt16();
            _population  = bCollisionDataFileReader.ReadInt64();
            _lifeExpectancy = bCollisionDataFileReader.ReadSingle();
            _gnp         = bCollisionDataFileReader.ReadInt32();
            _link        = bCollisionDataFileReader.ReadInt16();

            fCollisionDataFile.Seek(currentPosition, SeekOrigin.Begin);
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
            WriteRecord(bMDataFileWriter, -1);

        }

        //--------------------------------------------------------------------------
        /// <summary>
        /// Reads a record in collision file
        /// </summary>
        /// <param name="queryCode">Code that is being searched for</param>
        /// <returns>Array of byteoffset that was searched</returns>
        private int []SearchCollisionFile(string queryCode, ref int recordChecked)
        {
            List<int> byteOffSetList = new List<int>();
            bool recordFound = false;

            if (_link != -1)
            {
                do
                {
                    byteOffSetList.Add(ReadOneCollisionRecord(_link));

                    if (queryCode.ToUpper().CompareTo(new string(_code).ToUpper()) == 0)
                        recordFound = true;

                    ++recordChecked;

                } while (_link != -1 && recordFound == false);
            }

            return recordFound ? byteOffSetList.ToArray() : null;
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
            _link = -1;


        }

        //-----------------------------------------------------------------------
        /// <summary>
        /// Handles the collision data from main data file. And places new 
        /// information in the collision file.
        /// </summary>
        /// <param name="byteOffSet"></param>
        private void CollisionHandling(int MainDatabyteOffSet)
        {

            short oldLink = -1;
            int collisionByteOffSet = CalculateByteOffSet(nCollRec) - _sizeOfHeaderRec;

            fCollisionDataFile.Seek(collisionByteOffSet, SeekOrigin.Begin);

            //Save and replace link data
            fMainDataFile.Seek(MainDatabyteOffSet+_sizeOfDataRec-sizeof(short), SeekOrigin.Begin);
            oldLink = bMDataFileReader.ReadInt16();                    //Save old link
            fMainDataFile.Seek(-sizeof(short), SeekOrigin.Current);
            bMDataFileWriter.Write(nCollRec);                          //Place new link
            
            WriteRecord(bCollisionDataFileWriter, oldLink);
            
        }

        //------------------------------------------------------------------------
        /// <summary>
        /// Writes All data to a file based on its Writer function in use
        /// </summary>
        /// <param name="bw">What file structure is being written to</param>
        private void WriteRecord(BinaryWriter bw, short link)
        {
            bw.Write(_code);
            bw.Write(_name);
            bw.Write(_continent);
            bw.Write(_surfaceArea);
            bw.Write(_yearOfIndep);
            bw.Write(_population);
            bw.Write(_lifeExpectancy);
            bw.Write(_gnp);
            bw.Write(link);
        }

        //------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes a record based on its location it was found
        /// </summary>
        /// <param name="byteOffSet">A trail of offsets that lead back to start</param>
        /// <param name="recordCount">How many where counted</param>
        /// <param name="Code">Actual query code searched</param>

        private void TomeStoneRecord(int []byteOffSet, int recordCount, char []Code)
        {
            short oldLink;
            int mainFileByteOffSet = 0;


            switch(recordCount)
            {
                case 1:  //Code was found in the main File
                    fMainDataFile.Seek(byteOffSet[0], SeekOrigin.Begin);
                    ReadOneRecord();
                    fMainDataFile.Seek(byteOffSet[0], SeekOrigin.Begin);

                    if(_link != -1)
                    {
                        ReadOneCollisionRecord(_link); //Store the frist record in collision 
                    
                        WriteRecord(bMDataFileWriter, _link); //Replace main record with collision
                    }
                    else
                    {
                        _code = new char[_code.Length];
                        WriteRecord(bMDataFileWriter, _link); //TomeStone Code and replace record
                    }
                    break;
                case 2:  //Code was found in the first link from main to collision file
                    mainFileByteOffSet = CalculateByteOffSet(HashFunction(Code));

                    fMainDataFile.Seek(mainFileByteOffSet
                                             +_sizeOfDataRec-sizeof(short), SeekOrigin.Begin);
                    fCollisionDataFile.Seek(byteOffSet[byteOffSet.Length] 
                                             + _sizeOfDataRec - sizeof(short), SeekOrigin.Begin);

                    //Save Collisions data link.
                    oldLink = bCollisionDataFileReader.ReadInt16();

                    //Write Collisions link to maindata file
                    bMDataFileWriter.Write(oldLink);

                    fCollisionDataFile.Seek(byteOffSet[byteOffSet.Length], SeekOrigin.Begin);
                    ReadOneCollisionRecord();

                    //Tombstone code and link
                    _code = new char[_code.Length];
                    WriteRecord(bCollisionDataFileWriter, -1);

                    break;
                default: //Code was found somewhere in the collision file (not first)

                    //Go to the record that needs to be deleted
                    fCollisionDataFile.Seek(byteOffSet[byteOffSet.Length]
                                             + _sizeOfDataRec - sizeof(short), SeekOrigin.Begin);

                    //Read record into memory (including its important link RRN)
                    ReadOneCollisionRecord();

                    //Tombstone code
                    _code = new char[_code.Length];
                    WriteRecord(bCollisionDataFileWriter, -1); //Eliminate record

                    //Go to the record that linked to the one that needed to be deleted.
                    fCollisionDataFile.Seek(byteOffSet[byteOffSet.Length-1]
                                             + _sizeOfDataRec - sizeof(short), SeekOrigin.Begin);

                    //Replace its RRN with the one that was deleted
                    bCollisionDataFileWriter.Write(_link);


                    break;
            }
           
        }

    }
}
