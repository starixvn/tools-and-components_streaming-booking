namespace StreamingBooking.Models.IMG
{
    using System;

    public class EventModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public TournamentModel Tournament { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}