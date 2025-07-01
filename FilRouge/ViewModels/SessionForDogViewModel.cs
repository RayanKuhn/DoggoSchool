namespace FilRouge.ViewModels
{
    public class SessionForDogViewModel
    {
        public int SessionId { get; set; }

        public DateOnly CourseDate { get; set; }
        public TimeOnly CourseHour { get; set; }

        public string CourseName { get; set; }
        public string TeacherName { get; set; }
        public int DurationInMinutes { get; set; }

        public int MembersMax { get; set; }
        public int MembersRegistered { get; set; }

        public int PlacesRemaining => MembersMax - MembersRegistered;
        public TimeOnly CourseEndingTime => CourseHour.AddMinutes(DurationInMinutes);
        public bool IsRegistered { get; set; } = false;
        public int? AgeMin { get; set; }
        public int? AgeMax { get; set; }
        public int? HeightMin { get; set; }
        public int? HeightMax { get; set; }
        public decimal? WeightMin { get; set; }
        public decimal? WeightMax { get; set; }

        public DateOnly RegistrationDate { get; set; }

    }
}
