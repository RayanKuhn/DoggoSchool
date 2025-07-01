using System;
using FilRouge.Models;

namespace FilRouge.ViewModels
{
    public class RegistrationViewModel
    {
        public int RegisterId { get; set; }

        public string DogName { get; set; }

        public string CourseName { get; set; }

        public string TeacherName { get; set; }

        public DateOnly RegistrationDate { get; set; }

        public Status Status { get; set; }

        public int DogId { get; set; }
        public int CourseId { get; set; }
    }
}
