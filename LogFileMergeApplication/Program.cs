﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SICIDeviceLog;

namespace LogFileMergeApplication
{
    class Program
    {

        static void Main( string[] args )
        {
            SICIDeviceLog.SICIDeviceLog deviceLog = new SICIDeviceLog.SICIDeviceLog ( @"C:\Log", "SICI_DEVICE_LOG_TEST" );

            string CLIENT_LOG_FILEPATH = @"C:\Log\20170919110748-TEST_CLIENT_DEVICE_LOG.log";
            string SERVER_LOG_FILEPATH = @"C:\Log\20170919110754-TEST_SERVER_DEVICE_LOG.log";
            string NEW_LOG_FILEPATH = @"C:\Log\SICI Test Logs\1MB_SORTED_LOGS";

            List<LogEntry> logEntries = new List<LogEntry> ();

            if ( File.Exists ( CLIENT_LOG_FILEPATH ) && File.Exists ( SERVER_LOG_FILEPATH ) )
            {
                if ( File.Exists ( NEW_LOG_FILEPATH ) )
                {
                    File.Delete ( NEW_LOG_FILEPATH );
                }


                // Populate logEntries with all log entries from both log files
                using ( StreamReader clientSR = new StreamReader ( CLIENT_LOG_FILEPATH ) )
                using ( StreamReader serverSR = new StreamReader ( SERVER_LOG_FILEPATH ) )
                {
                    while ( clientSR.Peek () >= 0 )
                    {
                        LogEntry newLogEntry = MakeLogEntry ( clientSR.ReadLine () );
                        logEntries.Add ( newLogEntry );
                    }
                    while ( serverSR.Peek () >= 0 )
                    {
                        LogEntry newLogEntry = MakeLogEntry ( serverSR.ReadLine () );
                        logEntries.Add ( newLogEntry );
                    }
                }

                // Sort logEntries by dateTime field
                // Then need to create a single, merged log file with sorted log entries
                logEntries.Sort ( new Comparison<LogEntry> ( ( x, y ) => DateTime.Compare ( x.dateTime, y.dateTime ) ) );

                // Testing sort
                #region
                //foreach(LogEntry l in logEntries){
                //    Console.WriteLine(l.ToString());
                //    Console.ReadLine();
                //}
                #endregion


                // Creating new merged log file
                using ( StreamWriter sw = new StreamWriter ( NEW_LOG_FILEPATH ) )
                {
                    // Logic for keeping track of round trip message times
                    DateTime msgSent, msgReceived, msgSentBack, msgReturned;
                    msgSent = msgReceived = msgSentBack = msgReturned = new DateTime ();
                    TimeSpan fromClientToServerTime, timeBeforeSendBack, fromServerToClientTime, roundTripTime;
                    foreach ( LogEntry curLogEntry in logEntries )
                    {
                        // Write the log message to merged log
                        sw.WriteLine ( curLogEntry.ToString () );

                        switch ( curLogEntry.getLogData ()[2] )
                        {
                            case "Client sending message to server.":
                                msgSent = curLogEntry.dateTime;
                                break;
                            case "Server message received.":
                                msgReceived = curLogEntry.dateTime;
                                break;
                            case "Server returning message to client.":
                                msgSentBack = curLogEntry.dateTime;
                                break;
                            case "Message received back from server.":
                                // Additional functionality in order to process round trip
                                // as well as for logging the four recorded times 
                                msgReturned = curLogEntry.dateTime;
                                fromClientToServerTime = msgReceived - msgSent;
                                timeBeforeSendBack = msgSentBack - msgReceived;
                                fromServerToClientTime = msgReturned - msgSentBack;
                                roundTripTime = msgReturned - msgSent;
                                sw.WriteLine ( "Client -> Server Time: " + fromClientToServerTime.ToString () );
                                sw.WriteLine ( "Time on server:        " + timeBeforeSendBack.ToString () );
                                sw.WriteLine ( "Server -> Client Time: " + fromServerToClientTime.ToString () );
                                sw.WriteLine ( "Round Trip Time:       " + roundTripTime.ToString () );
                                sw.WriteLine ();

                                // Logging the values in the SICIDeviceLog
                                deviceLog.LogInfo ( String.Format ( "T1:{0},T2:{1},T3:{2},T4:{3},T2-T1:{4},T3-T2:{5},T4-T3:{6},T4-T1:{7}",
                                    msgSent, msgReceived, msgSentBack, msgReturned, fromClientToServerTime.ToString (),
                                    timeBeforeSendBack.ToString (), fromServerToClientTime.ToString (), roundTripTime.ToString () ) );
                                break;
                            default:
                                throw new ArgumentOutOfRangeException ();
                        }
                    }
                }


            }
            else
            {
                Console.WriteLine ( "ERROR: one or both log files does not exist." );
            }

        }

        private static LogEntry MakeLogEntry( string str )
        {
            // Testing string values
            #region
            //Console.WriteLine(str.Substring(0, 23));
            ////Console.WriteLine(str.Substring(5, 2));
            ////Console.WriteLine(str.Substring(8, 2));
            ////Console.WriteLine(str.Substring(11, 2));
            ////Console.WriteLine(str.Substring(14, 2));
            ////Console.WriteLine(str.Substring(17, 2));
            ////Console.WriteLine(str.Substring(20, 3));
            //Console.WriteLine(str.Substring(24, 4));
            //Console.WriteLine(str.Substring(29, 22));
            //Console.WriteLine(str.Substring(52));
            //Console.ReadLine();
            #endregion

            try
            {
                DateTime logEntryDateTime = new DateTime ( Convert.ToInt32 ( str.Substring ( 0, 4 ) ), Convert.ToInt32 ( str.Substring ( 5, 2 ) ),
                                Convert.ToInt32 ( str.Substring ( 8, 2 ) ), Convert.ToInt32 ( str.Substring ( 11, 2 ) ), Convert.ToInt32 ( str.Substring ( 14, 2 ) ),
                                Convert.ToInt32 ( str.Substring ( 17, 2 ) ), Convert.ToInt32 ( str.Substring ( 20, 3 ) ) );
                LogEntry newLogEntry = new LogEntry ( logEntryDateTime, str.Substring ( 24, 4 ), str.Substring ( 29, 22 ), str.Substring ( 52 ) );
                return newLogEntry;

            }
            catch ( Exception e )
            {
                Console.WriteLine ( e.Message );
                throw;
            }




        }
    }
}
