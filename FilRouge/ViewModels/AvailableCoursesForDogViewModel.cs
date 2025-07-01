using FilRouge.Models;
using System.Collections.Generic;

namespace FilRouge.ViewModels
{
    public class AvailableCoursesForDogViewModel
    {
        public Dog Dog { get; set; }

        public List<CourseForUserViewModel> CompatibleCourses { get; set; }
    }
}

