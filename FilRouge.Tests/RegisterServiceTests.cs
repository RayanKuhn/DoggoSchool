using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FilRouge.Models;
using FilRouge.Services;
using FilRouge.Services.Interfaces;
using FilRouge.Data;
using Microsoft.EntityFrameworkCore;

namespace FilRouge.Tests
{
    [TestClass]
    public class RegisterServiceTests
    {
        private Mock<IDogService> _mockDogService;
        private Mock<ICourseService> _mockCourseService;
        private RegisterService _sut;   // System Under Test

        [TestInitialize]
        public void Setup()
        {
            _mockDogService = new Mock<IDogService>();
            _mockCourseService = new Mock<ICourseService>();

            // Par défaut : 12 mois
            _mockDogService.Setup(ds => ds.GetAgeInMonths(It.IsAny<Dog>()))
                           .Returns(12);

            _sut = new RegisterService(context: null,
                                       dogService: _mockDogService.Object,
                                       courseService: _mockCourseService.Object);
        }

        [TestMethod]
        public void CanRegisterDogToCourse_ReturnsTrue_WhenDogMeetsAllConditions()
        {
            // Arrange
            var dog = new Dog { Height = 50, Weight = 20 };
            var course = new Course
            {
                AgeMin = 6,
                AgeMax = 24,
                HeightMin = 40,
                HeightMax = 60,
                WeightMin = 10,
                WeightMax = 30
            };

            // Act
            var result = _sut.CanRegisterDogToCourse(dog, course);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CanRegisterDogToCourse_ReturnsFalse_WhenDogTooYoung()
        {
            // Arrange – on override le mock pour ce test uniquement
            _mockDogService.Setup(ds => ds.GetAgeInMonths(It.IsAny<Dog>()))
                           .Returns(4);

            var dog = new Dog { Height = 50, Weight = 20 };
            var course = new Course
            {
                AgeMin = 10,
                AgeMax = 15,
                HeightMin = 40,
                HeightMax = 60,
                WeightMin = 10,
                WeightMax = 30
            };

            // Act
            var result = _sut.CanRegisterDogToCourse(dog, course);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TryRegisterDogToCourseAsync_ReturnsTrue_AndAddsRegister()
        {
            // ─── Arrange (DbContext InMemory) ─────────────────────────────────
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var db = new ApplicationDbContext(options);

            var userId = "user123";

            var person = new Person
            {
                PersonId = 1,
                IdentityUserId = userId,
                FirstName = "Jean",
                LastName = "Dupont",
                Address = "42 rue des Tests",
                PhoneNumber = "0102030405",
                BirthDate = DateTime.Today.AddYears(-25),
                Role = Role.User
            };

            var dog = new Dog
            {
                DogId = 1,
                Person = person,
                PersonId = person.PersonId,
                Name = "Rex",
                HealthIssues = "",
                Height = 50,
                Weight = 20,
                BirthDate = DateTime.Today.AddMonths(-12)
            };

            var prof = new Person
            {
                PersonId = 99,
                IdentityUserId = "teacher1",
                FirstName = "Prof",
                LastName = "Testeur",
                Address = "1 rue du Prof",
                PhoneNumber = "0600000000",
                BirthDate = DateTime.Today.AddYears(-30),
                Role = Role.Teacher
            };

            db.Persons.Add(prof);

            var course = new Course
            {
                CourseId = 1,
                Name = "Agility Débutant",
                IsValidatedByAdmin = true,
                PersonId = 99,               
                AgeMin = 6,
                AgeMax = 36,
                HeightMin = 40,
                HeightMax = 60,
                WeightMin = 10,
                WeightMax = 30
            };

            db.Persons.Add(person);
            db.Dogs.Add(dog);
            db.Courses.Add(course);
            await db.SaveChangesAsync();

            // ─── Mocks & SUT ──────────────────────────────────────────────────
            var dogServiceMock = new Mock<IDogService>();
            var courseServiceMock = new Mock<ICourseService>();

            dogServiceMock.Setup(s => s.GetAgeInMonths(It.IsAny<Dog>()))
                          .Returns(12);            // âge OK

            var sut = new RegisterService(db, dogServiceMock.Object, courseServiceMock.Object);

            // ─── Act ─────────────────────────────────────────────────────────
            var result = await sut.TryRegisterDogToCourseAsync(dogId: 1, courseId: 1, identityUserId: userId);

            // ─── Assert ──────────────────────────────────────────────────────
            Assert.IsTrue(result.Success, "L'inscription devrait être acceptée.");
            Assert.IsNull(result.ErrorMessage);

            var register = await db.Registers.FirstOrDefaultAsync();
            Assert.IsNotNull(register, "Une inscription doit être créée dans la base.");
            Assert.AreEqual(Status.Registered, register.Status);
            Assert.AreEqual(1, register.DogId);
            Assert.AreEqual(1, register.CourseId);
        }

        [TestMethod]
        public async Task TryRegisterDogToCourseAsync_ReturnsFalse_WhenDogNotOwnedByUser()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var db = new ApplicationDbContext(options);

            var myUserId = "userA";
            var otherUserId = "userB";               // propriétaire réel du chien

            var ownerB = new Person
            {
                PersonId = 2,
                IdentityUserId = otherUserId,
                FirstName = "B",
                LastName = "B",
                Address = "B",
                PhoneNumber = "B",
                BirthDate = DateTime.Today.AddYears(-20),
                Role = Role.User
            };

            var dog = new Dog
            {
                DogId = 10,
                Person = ownerB,
                PersonId = 2,
                Name = "Rex",
                HealthIssues = "",
                Height = 50,
                Weight = 20,
                BirthDate = DateTime.Today.AddMonths(-12)
            };
            var prof = new Person
            {
                PersonId = 99,
                IdentityUserId = "teacherX",
                FirstName = "Prof",
                LastName = "X",
                Address = "1 rue du Prof",
                PhoneNumber = "0600000000",
                BirthDate = DateTime.Today.AddYears(-30),
                Role = Role.Teacher
            };

            db.Persons.Add(prof);

            var course = new Course
            {
                CourseId = 10,
                Name = "Cours",
                IsValidatedByAdmin = true,
                PersonId = 99,
                AgeMin = 6,
                AgeMax = 36,
                HeightMin = 40,
                HeightMax = 60,
                WeightMin = 10,
                WeightMax = 30
            };

            db.Persons.Add(ownerB);
            db.Dogs.Add(dog);
            db.Courses.Add(course);
            await db.SaveChangesAsync();

            var mockDogSvc = new Mock<IDogService>();
            mockDogSvc.Setup(x => x.GetAgeInMonths(It.IsAny<Dog>())).Returns(12);

            var sut = new RegisterService(db, mockDogSvc.Object, new Mock<ICourseService>().Object);

            // Act
            var res = await sut.TryRegisterDogToCourseAsync(dog.DogId, course.CourseId, myUserId);

            // Assert
            Assert.IsFalse(res.Success);
            Assert.AreEqual("Ce chien ne vous appartient pas.", res.ErrorMessage);
            Assert.AreEqual(0, db.Registers.Count());
        }

        [TestMethod]
        public async Task TryRegisterDogToCourseAsync_ReturnsFalse_WhenCourseNotFound()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var db = new ApplicationDbContext(options);

            var userId = "user1";
            var person = new Person
            {
                PersonId = 1,
                IdentityUserId = userId,
                FirstName = "A",
                LastName = "A",
                Address = "A",
                PhoneNumber = "A",
                BirthDate = DateTime.Today.AddYears(-20),
                Role = Role.User
            };
            var dog = new Dog
            {
                DogId = 1,
                Person = person,
                PersonId = 1,
                Name = "Rex",
                HealthIssues = "",
                Height = 50,
                Weight = 20,
                BirthDate = DateTime.Today.AddMonths(-12)
            };

            db.Persons.Add(person);
            db.Dogs.Add(dog);
            await db.SaveChangesAsync();

            var mockDogSvc = new Mock<IDogService>();
            mockDogSvc.Setup(x => x.GetAgeInMonths(It.IsAny<Dog>())).Returns(12);

            var sut = new RegisterService(db, mockDogSvc.Object, new Mock<ICourseService>().Object);

            var res = await sut.TryRegisterDogToCourseAsync(dog.DogId, courseId: 999, identityUserId: userId);

            Assert.IsFalse(res.Success);
            Assert.AreEqual("Ce cours est introuvable ou non validé.", res.ErrorMessage);
        }

        [TestMethod]
        public async Task TryRegisterDogToCourseAsync_ReturnsFalse_WhenCourseNotValidated()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var db = new ApplicationDbContext(options);

            var userId = "user1";
            var person = new Person
            {
                PersonId = 1,
                IdentityUserId = userId,
                FirstName = "A",
                LastName = "A",
                Address = "A",
                PhoneNumber = "A",
                BirthDate = DateTime.Today.AddYears(-20),
                Role = Role.User
            };
            var dog = new Dog
            {
                DogId = 1,
                Person = person,
                PersonId = 1,
                Name = "Rex",
                HealthIssues = "",
                Height = 50,
                Weight = 20,
                BirthDate = DateTime.Today.AddMonths(-12)
            };
            var prof = new Person
            {
                PersonId = 99,
                IdentityUserId = "teacherX",
                FirstName = "Prof",
                LastName = "X",
                Address = "1 rue du Prof",
                PhoneNumber = "0600000000",
                BirthDate = DateTime.Today.AddYears(-30),
                Role = Role.Teacher
            };

            db.Persons.Add(prof);

            var course = new Course
            {
                CourseId = 1,
                Name = "Cours",
                IsValidatedByAdmin = false,
                PersonId = 99,
                AgeMin = 6,
                AgeMax = 36,
                HeightMin = 40,
                HeightMax = 60,
                WeightMin = 10,
                WeightMax = 30
            };

            db.Persons.Add(person);
            db.Dogs.Add(dog);
            db.Courses.Add(course);
            await db.SaveChangesAsync();

            var mockDogSvc = new Mock<IDogService>();
            mockDogSvc.Setup(x => x.GetAgeInMonths(It.IsAny<Dog>())).Returns(12);

            var sut = new RegisterService(db, mockDogSvc.Object, new Mock<ICourseService>().Object);

            var res = await sut.TryRegisterDogToCourseAsync(dog.DogId, course.CourseId, userId);

            Assert.IsFalse(res.Success);
            Assert.AreEqual("Ce cours est introuvable ou non validé.", res.ErrorMessage);
        }


        [TestMethod]
        public async Task TryRegisterDogToCourseAsync_ReturnsFalse_WhenDogDoesNotMeetCriteria()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var db = new ApplicationDbContext(options);

            var userId = "user1";
            var person = new Person
            {
                PersonId = 1,
                IdentityUserId = userId,
                FirstName = "A",
                LastName = "A",
                Address = "A",
                PhoneNumber = "A",
                BirthDate = DateTime.Today.AddYears(-20),
                Role = Role.User
            };
            var heavyDog = new Dog
            {
                DogId = 1,
                Person = person,
                PersonId = 1,
                Name = "Big",
                HealthIssues = "",
                Height = 70,
                Weight = 45,
                BirthDate = DateTime.Today.AddMonths(-12)
            };
            var prof = new Person
            {
                PersonId = 99,
                IdentityUserId = "teacherX",
                FirstName = "Prof",
                LastName = "X",
                Address = "1 rue du Prof",
                PhoneNumber = "0600000000",
                BirthDate = DateTime.Today.AddYears(-30),
                Role = Role.Teacher
            };

            db.Persons.Add(prof);
            var course = new Course
            {
                CourseId = 1,
                Name = "Cours",
                IsValidatedByAdmin = true,
                PersonId = 99,
                AgeMin = 6,
                AgeMax = 36,
                HeightMin = 40,
                HeightMax = 60,
                WeightMin = 10,
                WeightMax = 30
            };

            db.Persons.Add(person);
            db.Dogs.Add(heavyDog);
            db.Courses.Add(course);
            await db.SaveChangesAsync();

            var mockDogSvc = new Mock<IDogService>();
            mockDogSvc.Setup(x => x.GetAgeInMonths(It.IsAny<Dog>())).Returns(12);

            var sut = new RegisterService(db, mockDogSvc.Object, new Mock<ICourseService>().Object);

            var res = await sut.TryRegisterDogToCourseAsync(heavyDog.DogId, course.CourseId, userId);

            Assert.IsFalse(res.Success);
            Assert.AreEqual("Votre chien ne remplit pas les conditions pour ce cours.", res.ErrorMessage);
        }


        [TestMethod]
        public async Task TryRegisterDogToCourseAsync_ReturnsFalse_WhenDogAlreadyRegistered()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            using var db = new ApplicationDbContext(options);

            var userId = "user1";
            var person = new Person
            {
                PersonId = 1,
                IdentityUserId = userId,
                FirstName = "A",
                LastName = "A",
                Address = "A",
                PhoneNumber = "A",
                BirthDate = DateTime.Today.AddYears(-20),
                Role = Role.User
            };
            var dog = new Dog
            {
                DogId = 1,
                Person = person,
                PersonId = 1,
                Name = "Rex",
                HealthIssues = "",
                Height = 50,
                Weight = 20,
                BirthDate = DateTime.Today.AddMonths(-12)
            };
            var prof = new Person
            {
                PersonId = 99,
                IdentityUserId = "teacherX",
                FirstName = "Prof",
                LastName = "X",
                Address = "1 rue du Prof",
                PhoneNumber = "0600000000",
                BirthDate = DateTime.Today.AddYears(-30),
                Role = Role.Teacher
            };

            db.Persons.Add(prof);
            var course = new Course
            {
                CourseId = 1,
                Name = "Cours",
                IsValidatedByAdmin = true,
                PersonId = 99,
                AgeMin = 6,
                AgeMax = 36,
                HeightMin = 40,
                HeightMax = 60,
                WeightMin = 10,
                WeightMax = 30
            };

            var existingRegister = new Register
            {
                RegisterId = 1,
                DogId = dog.DogId,
                CourseId = course.CourseId,
                Status = Status.Registered
            };

            db.Persons.Add(person);
            db.Dogs.Add(dog);
            db.Courses.Add(course);
            db.Registers.Add(existingRegister);
            await db.SaveChangesAsync();

            var mockDogSvc = new Mock<IDogService>();
            mockDogSvc.Setup(x => x.GetAgeInMonths(It.IsAny<Dog>())).Returns(12);

            var sut = new RegisterService(db, mockDogSvc.Object, new Mock<ICourseService>().Object);

            var res = await sut.TryRegisterDogToCourseAsync(dog.DogId, course.CourseId, userId);

            Assert.IsFalse(res.Success);
            Assert.AreEqual("Ce chien est déjà inscrit à ce cours.", res.ErrorMessage);
        }
    }
}

