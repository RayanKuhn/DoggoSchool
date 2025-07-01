using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using FilRouge.Data;
using FilRouge.Models;
using FilRouge.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FilRouge.Tests
{
    [TestClass]
    public class ParticipateServiceTests
    {
        /// <summary>
        /// Le chien est inscrit au cours, une session existe dans la semaine courante.
        /// La méthode doit retourner cette session (IsRegistered = false car le chien
        /// n’est pas encore inscrit à la session elle-même).
        /// </summary>
        [TestMethod]
        public async Task GetAvailableSessionsForDogAsync_ReturnsSession_WhenDogIsRegisteredToCourse()
        {
            // ─── Arrange : base InMemory isolée ─────────────────────────────────────
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var db = new ApplicationDbContext(options);

            // 1) Création d’un utilisateur "classique"
            var user = new Person
            {
                PersonId = 1,
                IdentityUserId = "user1",
                FirstName = "Alice",
                LastName = "Tester",
                Address = "1 rue des Tests",
                PhoneNumber = "0600000000",
                BirthDate = DateTime.Today.AddYears(-25),
                Role = Role.User
            };

            // 2) Création d’un prof (pour que PersonId=99 existe)
            var teacher = new Person
            {
                PersonId = 99,
                IdentityUserId = "teacher1",
                FirstName = "Bob",
                LastName = "Prof",
                Address = "2 rue du Prof",
                PhoneNumber = "0700000000",
                BirthDate = DateTime.Today.AddYears(-30),
                Role = Role.Teacher
            };

            // 3) Le chien de l’utilisateur
            var dog = new Dog
            {
                DogId = 1,
                Name = "Rex",
                HealthIssues = "",
                Height = 50,
                Weight = 20,
                BirthDate = DateTime.Today.AddMonths(-12),
                Person = user,
                PersonId = user.PersonId
            };

            // 4) Le cours validé
            var course = new Course
            {
                CourseId = 1,
                Name = "Obé-Débutant",
                IsValidatedByAdmin = true,
                PersonId = teacher.PersonId,
                AgeMin = 6,
                AgeMax = 36,
                HeightMin = 30,
                HeightMax = 60,
                WeightMin = 10,
                WeightMax = 40
            };

            // 5) Inscription chien ↔ cours (Register)
            var register = new Register
            {
                RegisterId = 1,
                DogId = dog.DogId,
                CourseId = course.CourseId,
                Status = Status.Registered,
                RegistrationDate = DateOnly.FromDateTime(DateTime.Today)
            };

            // 6) Une session dans la semaine en cours
            var today = DateOnly.FromDateTime(DateTime.Today);
            var sessionInWeek = new Session
            {
                SessionId = 1,
                CourseId = course.CourseId,
                CourseDate = today,           
                CourseHour = new TimeOnly(10, 0),
                MembersMax = 5,
                MembersRegistered = 0
            };

            db.Persons.AddRange(user, teacher);
            db.Dogs.Add(dog);
            db.Courses.Add(course);
            db.Registers.Add(register);
            db.Sessions.Add(sessionInWeek);
            await db.SaveChangesAsync();

            var sut = new ParticipateService(db);

            // ─── Act : semaine courante (weekOffset = 0) ──────────────────────────
            var sessions = await sut.GetAvailableSessionsForDogAsync(dog, weekOffset: 0);

            // ─── Assert ───────────────────────────────────────────────────────────
            Assert.AreEqual(1, sessions.Count, "Une seule session doit être retournée.");
            var vm = sessions.First();

            Assert.AreEqual(sessionInWeek.SessionId, vm.SessionId);
            Assert.IsFalse(vm.IsRegistered, "Le chien n'est pas encore inscrit à la session, IsRegistered doit être false.");
        }

        [TestMethod]
        public async Task GetAvailableSessionsForDogAsync_ReturnsEmpty_WhenSessionIsOutsideWeek()
        {
            // ─── Arrange ────────────────────────────────────────────────────────
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var db = new ApplicationDbContext(options);

            var user = new Person
            {
                PersonId = 1,
                IdentityUserId = "user123",
                FirstName = "Test",
                LastName = "User",
                Address = "1 test street",
                PhoneNumber = "0101010101",
                BirthDate = DateTime.Today.AddYears(-30),
                Role = Role.User
            };

            var teacher = new Person
            {
                PersonId = 99,
                IdentityUserId = "teacher",
                FirstName = "Prof",
                LastName = "Tester",
                Address = "2 prof street",
                PhoneNumber = "0202020202",
                BirthDate = DateTime.Today.AddYears(-40),
                Role = Role.Teacher
            };

            var dog = new Dog
            {
                DogId = 1,
                Name = "Rex",
                HealthIssues = "",
                Height = 50,
                Weight = 20,
                BirthDate = DateTime.Today.AddMonths(-12),
                Person = user,
                PersonId = user.PersonId
            };

            var course = new Course
            {
                CourseId = 1,
                Name = "Agility Avancé",
                IsValidatedByAdmin = true,
                PersonId = teacher.PersonId,
                AgeMin = 6,
                AgeMax = 36,
                HeightMin = 40,
                HeightMax = 60,
                WeightMin = 10,
                WeightMax = 30
            };

            var register = new Register
            {
                RegisterId = 1,
                DogId = dog.DogId,
                CourseId = course.CourseId,
                Status = Status.Registered,
                RegistrationDate = DateOnly.FromDateTime(DateTime.Today)
            };

            // Session dans +2 semaines → hors scope de weekOffset = 0
            var sessionOutOfWeek = new Session
            {
                SessionId = 1,
                CourseId = course.CourseId,
                CourseDate = DateOnly.FromDateTime(DateTime.Today).AddDays(14),
                CourseHour = new TimeOnly(14, 0),
                MembersMax = 10,
                MembersRegistered = 0
            };

            db.Persons.AddRange(user, teacher);
            db.Dogs.Add(dog);
            db.Courses.Add(course);
            db.Registers.Add(register);
            db.Sessions.Add(sessionOutOfWeek);
            await db.SaveChangesAsync();

            var sut = new ParticipateService(db);

            // ─── Act ────────────────────────────────────────────────────────────
            var sessions = await sut.GetAvailableSessionsForDogAsync(dog, weekOffset: 0);

            // ─── Assert ─────────────────────────────────────────────────────────
            Assert.AreEqual(0, sessions.Count, "Aucune session ne doit apparaître si elle est hors de la semaine demandée.");
        }

        [TestMethod]
        public async Task GetAvailableSessionsForDogAsync_ReturnsEmpty_WhenDogIsNotRegisteredToCourse()
        {
            // ─── Arrange ────────────────────────────────────────────────────────
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var db = new ApplicationDbContext(options);

            var user = new Person
            {
                PersonId = 1,
                IdentityUserId = "user123",
                FirstName = "Test",
                LastName = "User",
                Address = "1 test street",
                PhoneNumber = "0101010101",
                BirthDate = DateTime.Today.AddYears(-30),
                Role = Role.User
            };

            var teacher = new Person
            {
                PersonId = 99,
                IdentityUserId = "teacher",
                FirstName = "Prof",
                LastName = "Tester",
                Address = "2 prof street",
                PhoneNumber = "0202020202",
                BirthDate = DateTime.Today.AddYears(-40),
                Role = Role.Teacher
            };

            var dog = new Dog
            {
                DogId = 1,
                Name = "Rex",
                HealthIssues = "",
                Height = 50,
                Weight = 20,
                BirthDate = DateTime.Today.AddMonths(-12),
                Person = user,
                PersonId = user.PersonId
            };

            var course = new Course
            {
                CourseId = 1,
                Name = "Agility Confirmé",
                IsValidatedByAdmin = true,
                PersonId = teacher.PersonId,
                AgeMin = 6,
                AgeMax = 36,
                HeightMin = 40,
                HeightMax = 60,
                WeightMin = 10,
                WeightMax = 30
            };

            var session = new Session
            {
                SessionId = 1,
                CourseId = course.CourseId,
                CourseDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                CourseHour = new TimeOnly(15, 0),
                MembersMax = 10,
                MembersRegistered = 0
            };

            db.Persons.AddRange(user, teacher);
            db.Dogs.Add(dog);
            db.Courses.Add(course);
            db.Sessions.Add(session);
            await db.SaveChangesAsync();

            var sut = new ParticipateService(db);

            // ─── Act ────────────────────────────────────────────────────────────
            var sessions = await sut.GetAvailableSessionsForDogAsync(dog);

            // ─── Assert ─────────────────────────────────────────────────────────
            Assert.AreEqual(0, sessions.Count, "Le chien ne devrait voir aucune session s’il n’est pas inscrit au cours.");
        }

        [TestMethod]
        public async Task GetAvailableSessionsForDogAsync_MarksSessionAsRegistered_WhenParticipationExists()
        {
            // ─── Arrange ────────────────────────────────────────────────────────
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            using var db = new ApplicationDbContext(options);

            var user = new Person
            {
                PersonId = 1,
                IdentityUserId = "user123",
                FirstName = "Test",
                LastName = "User",
                Address = "1 rue test",
                PhoneNumber = "0101010101",
                BirthDate = DateTime.Today.AddYears(-30),
                Role = Role.User
            };

            var teacher = new Person
            {
                PersonId = 99,
                IdentityUserId = "teacher123",
                FirstName = "Prof",
                LastName = "Tester",
                Address = "2 rue prof",
                PhoneNumber = "0202020202",
                BirthDate = DateTime.Today.AddYears(-40),
                Role = Role.Teacher
            };

            var dog = new Dog
            {
                DogId = 1,
                Name = "Rex",
                HealthIssues = "",
                Height = 50,
                Weight = 20,
                BirthDate = DateTime.Today.AddMonths(-12),
                Person = user,
                PersonId = user.PersonId
            };

            var course = new Course
            {
                CourseId = 1,
                Name = "Agility Pro",
                IsValidatedByAdmin = true,
                PersonId = teacher.PersonId,
                AgeMin = 6,
                AgeMax = 36,
                HeightMin = 40,
                HeightMax = 60,
                WeightMin = 10,
                WeightMax = 30
            };

            var session = new Session
            {
                SessionId = 1,
                CourseId = course.CourseId,
                CourseDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                CourseHour = new TimeOnly(14, 0),
                MembersMax = 10,
                MembersRegistered = 0
            };

            var register = new Register
            {
                DogId = dog.DogId,
                CourseId = course.CourseId,
                Status = Status.Registered,
                RegistrationDate = DateOnly.FromDateTime(DateTime.Today)
            };

            var participation = new Participate
            {
                DogId = dog.DogId,
                SessionId = session.SessionId,
                IsRegistered = true
            };

            db.Persons.AddRange(user, teacher);
            db.Dogs.Add(dog);
            db.Courses.Add(course);
            db.Sessions.Add(session);
            db.Registers.Add(register);
            db.Participations.Add(participation);
            await db.SaveChangesAsync();

            var sut = new ParticipateService(db);

            // ─── Act ────────────────────────────────────────────────────────────
            var sessions = await sut.GetAvailableSessionsForDogAsync(dog);

            // ─── Assert ─────────────────────────────────────────────────────────
            Assert.AreEqual(1, sessions.Count, "Une session devrait être visible.");
            Assert.IsTrue(sessions[0].IsRegistered, "Le champ IsRegistered devrait être à true.");
        }


    }
}
