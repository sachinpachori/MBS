using OFX_MBS.BL;
using OFX_MBS.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace OFX_MBS
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Read entire text file.
                Console.WriteLine("Please enter your text file path for meeting bookings (e.g., ~\\MeetingBooking\\meetingbooking.txt):");
                string filePath = Console.ReadLine();
                string fileString = File.ReadAllText(filePath);                
                              
                ProcessMeetingRequest oPMR = new ProcessMeetingRequest(fileString);
                List<Meeting> bookedMeetings = oPMR.BookMeeting();
                oPMR.WriteOutput(bookedMeetings);
                Console.WriteLine("\r");                    
                Console.WriteLine("Do you want to continue [Y/N]?");
                if (Console.ReadLine().ToUpper() == "Y")
                {
                    Console.WriteLine("\r");
                    Main(args);
                }
                else
                    Environment.Exit(0);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("MeetingBookingSystem Application Exception", ex.Message + " Trace" + ex.StackTrace, EventLogEntryType.Error, 121, short.MaxValue);
                Console.WriteLine("Error occured while processing meeting request. Please contact system administrator.");
                Console.ReadLine();
            }
        }    
    }   
}