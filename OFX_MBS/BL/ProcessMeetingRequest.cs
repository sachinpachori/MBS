using OFX_MBS.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OFX_MBS.BL
{
    class ProcessMeetingRequest
    {
        private string _fileString;
        private static int officeStartTimeInSeconds = 0;
        private static int officeEndTimeInSeconds = 0;

        public ProcessMeetingRequest(string fileString)
        {
            _fileString = fileString;
        }

        #region public methods
        /// <summary>
        /// Method to do the meeting booking
        /// </summary>
        /// <returns>List of meeting objects</returns>
        public List<Meeting> BookMeeting()
        {
            string[] temp = _fileString.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            var officeTime = temp[0];

            officeStartTimeInSeconds = GetOfficeStartTimeInSeconds(officeTime.Substring(0, 4));
            officeEndTimeInSeconds = GetOfficeEndTimeInSeconds(officeTime.Substring(5, 4));

            //List of input meeting requests
            List<Meeting> lstMeetingsInput = GetMeetingCollectionInput(temp);
            
            //List of input meeting requests grouped on "meeting start day" wise
            var lstGroupedonDate = lstMeetingsInput.GroupBy(x => x.MeetingStartDate).Select(x => x.ToList()).ToList();
            var bookingConfirmedList = new List<Meeting>();
            var lstTemp = new List<Meeting>();

            //Create List of Meetings that are confirmed
            foreach (var rec in lstGroupedonDate)
            {
                var overlaps = (from x1 in rec
                                from x2 in rec
                                where !Equals(x1, x2) // Don’t match the same object.
                                where x1.MeetingStartTime <= x2.MeetingEndTime && x1.MeetingEndTime > x2.MeetingStartTime   //check intersections
                                select x2).Distinct();

                lstTemp = overlaps.OrderBy(x => x.RequestSubmissionTime).ToList();
                lstTemp[0].MeetingBooked = true;
                bookingConfirmedList.Add(lstTemp[0]);

                var results = rec.Except(overlaps).ToList();
                foreach (var item in results)
                {
                    item.MeetingBooked = true;
                    bookingConfirmedList.Add(item);
                }
            }

            return bookingConfirmedList;
        }

        /// <summary>
        /// Method to print the booked meetings
        /// </summary>
        /// <param name="bookedMeetings"></param>
        public void WriteOutput(List<Meeting> bookedMeetings)
        {
            var orderedList = bookedMeetings.OrderBy(x => x.MeetingStartDate).GroupBy(x => x.MeetingStartDate).Select(x => x.ToList()).ToList();
            Console.WriteLine("\r");
            Console.WriteLine("Following meetings are booked:");
           
            foreach (var mtg in orderedList)
            {
                Console.WriteLine("\r");
                Console.WriteLine(mtg[0].MeetingStartDate.Date.ToString("yyyy-MM-dd"));
                
                foreach (var item in mtg)
                {                   
                    Console.WriteLine($"{item.MeetingStartTime.ToString("HH:mm")} {item.MeetingEndTime.ToString("HH:mm")}");
                    Console.WriteLine(item.EmployeeID);
                }
            }           
        }
        #endregion

        #region private methods
        /// <summary>
        /// Method to get the inout collection of meetings to be booked
        /// </summary>
        /// <param name="meetingsData"></param>
        /// <returns>List of meeting objects</returns>
        private List<Meeting> GetMeetingCollectionInput(string[] meetingsData)
        {
            List<Meeting> lstMeetings = new List<Meeting>();
            for (int count = 1; count < meetingsData.Length - 1;)
            {
                if (meetingsData[count] != string.Empty)
                {
                    Meeting objMeeting = new Meeting(DateTime.Parse(meetingsData[count]), meetingsData[count + 1], GetMeetingRequestDetails(meetingsData[count + 2]));
                    if (ValidateMeetingForOfficeHours(objMeeting, officeStartTimeInSeconds, officeEndTimeInSeconds) != null)
                        lstMeetings.Add(objMeeting);
                    count += 3;
                }
            }
            return lstMeetings;
        }
        
        /// <summary>
        /// Method to validate if the given meeting is in the given office hours
        /// </summary>
        /// <param name="mtg"></param>
        /// <param name="offieStartTimeInSeconds"></param>
        /// <param name="officeEndTimeInSeconds"></param>
        /// <returns>meeting or null object</returns>
        private Meeting ValidateMeetingForOfficeHours(Meeting mtg, int offieStartTimeInSeconds, int officeEndTimeInSeconds)
        {
            if (!(ConvertToSeconds(mtg.MeetingStartTime) >= offieStartTimeInSeconds && ConvertToSeconds(mtg.MeetingEndTime) <= officeEndTimeInSeconds))
                return null;
            return mtg;
        }

        /// <summary>
        /// Method to get the meeting request details
        /// </summary>
        /// <param name="meetingStartTime"></param>
        /// <returns>string array</returns>
        private string[] GetMeetingRequestDetails(string meetingStartTime)
        {
            string[] strArrTemp = { meetingStartTime.Substring(0, meetingStartTime.Length - 3),
            meetingStartTime.Substring(meetingStartTime.Length - 3, 3)
            };
            return strArrTemp;
        }

        /// <summary>
        /// Method to get the given office start time in seconds
        /// </summary>
        /// <param name="offStartTime"></param>
        /// <returns>int</returns>
        private int GetOfficeStartTimeInSeconds(string offStartTime)
        {
            int officeStartTimeHour = int.Parse(offStartTime.Substring(0, 2));
            int officeStartTimeMinutes = int.Parse(offStartTime.Substring(2, 2));
            return (officeStartTimeHour * 60 * 60) + (officeStartTimeMinutes * 60);
        }

        /// <summary>
        /// Method to get the given office end time in seconds
        /// </summary>
        /// <param name="offEndTime"></param>
        /// <returns>int</returns>
        private int GetOfficeEndTimeInSeconds(string offEndTime)
        {
            int officeEndTimeHour = int.Parse(offEndTime.Substring(0, 2));
            int officeEndTimeMinutes = int.Parse(offEndTime.Substring(2, 2));
            return (officeEndTimeHour * 60 * 60) + (officeEndTimeMinutes * 60);
        }

        /// <summary>
        /// Method to convert given time in seconds
        /// </summary>
        /// <param name="mtgTime"></param>
        /// <returns>int</returns>
        private int ConvertToSeconds(DateTime mtgTime)
        {
            return (mtgTime.Hour * 60 * 60) + (mtgTime.Minute * 60);
        }
        #endregion
    }
}