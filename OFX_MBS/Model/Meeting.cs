using System;

namespace OFX_MBS.Model
{
    class Meeting
    {      
        public string EmployeeID { get; set; }
        public bool MeetingBooked { get; set; }
        public int MeetingDuration { get; set; }
        public DateTime MeetingStartTime { get; set; }
        public DateTime MeetingEndTime { get; set; }
        public DateTime MeetingStartDate { get; set; }
        public DateTime RequestSubmissionTime { get; set; }

        public Meeting(DateTime reqSubTime, string empID, string[] meetingRequestDtls)
        {
            EmployeeID = empID.Trim();
            RequestSubmissionTime = reqSubTime;
            MeetingDuration = int.Parse(meetingRequestDtls[1].Trim());
            MeetingBooked = false;
            MeetingStartDate = DateTime.Parse(meetingRequestDtls[0]).Date;
            MeetingStartTime = DateTime.Parse(meetingRequestDtls[0]);
            MeetingEndTime = MeetingStartTime.AddHours(MeetingDuration);           
        }
    }
}
