namespace FilRouge.ViewModels
{
    public class CourseForUserViewModel
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public int DurationInMinutes { get; set; }
        public string TeacherName { get; set; }

        public int? AgeMin { get; set; }
        public int? AgeMax { get; set; }
        public int? HeightMin { get; set; }
        public int? HeightMax { get; set; }
        public decimal? WeightMin { get; set; }
        public decimal? WeightMax { get; set; }

    }

}
