﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class UserServiceTest
    {
        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext context;
        private IEntityRepository<User> repo;
        private IUserService service;
        private Mock<IStringLocalizer<SharedResource>> localizer;
        private Mock<ILogger> logger;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTestDB");

            options = builder.Options;
            context = new OutOfSchoolDbContext(options);
            localizer = new Mock<IStringLocalizer<SharedResource>>();
            repo = new EntityRepository<User>(context);
            logger = new Mock<ILogger>();
            service = new UserService(repo, logger.Object, localizer.Object);

            SeedDatabase();
        }

        [Test]
        [Order(1)]
        public async Task GetAll_WhenCalled_ReturnsAllEntities()
        {
            // Arrange
            var expected = await repo.GetAll();

            // Act
            var result = await service.GetAll().ConfigureAwait(false);

            // Assert
            Assert.That(expected.ToList().Count(), Is.EqualTo(result.Count()));
        }

        [Test]
        [Order(2)]
        [TestCase("cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c")]
        public async Task GetById_WhenIdIsValid_ReturnsEntity(string id)
        {
            // Arrange
            Expression<Func<User, bool>> filter = p => p.Id == id;
            var expected = await repo.GetByFilter(filter).ConfigureAwait(false);

            // Act
            var result = await service.GetById(id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.FirstOrDefault().Id, result.Id);
        }

        [Test]
        [Order(3)]
        [TestCase("Invalid Id")]
        public void GetById_WhenIdIsInvalid_ThrowsArgumentException(string id)
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await service.GetById(id).ConfigureAwait(false));
        }

        private void SeedDatabase()
        {
            using var context = new OutOfSchoolDbContext(options);
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var users = new List<User>()
                {
                   new User()
                   {
                        Id = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c",
                        CreatingTime = default,
                        LastLogin = default,
                        MiddleName = "MiddleName1",
                        FirstName = "FirstName1",
                        LastName = "LastName1",
                        UserName = "user1@gmail.com",
                        NormalizedUserName = "USER1@GMAIL.COM",
                        Email = "user1@gmail.com",
                        NormalizedEmail = "USER1@GMAIL.COM",
                        EmailConfirmed = false,
                        PasswordHash = "AQAAAAECcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                        SecurityStamp = "   CCCJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                        ConcurrencyStamp = "cb54f60f-6282-4416-874c-d1edce844d07",
                        PhoneNumber = "0965679725",
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = true,
                        AccessFailedCount = 0,
                   },
                   new User()
                   {
                        Id = "MM4a6876a-77fb-4bvse-9c78-a0880286ae3c",
                        CreatingTime = default,
                        LastLogin = default,
                        MiddleName = "MiddleName2",
                        FirstName = "FirstName2",
                        LastName = "LastName2",
                        UserName = "user2@gmail.com",
                        NormalizedUserName = "USER2@GMAIL.COM",
                        Email = "user2@gmail.com",
                        NormalizedEmail = "USER2@GMAIL.COM",
                        EmailConfirmed = false,
                        PasswordHash = "AQAAAAEAACcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                        SecurityStamp = "WGWJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                        ConcurrencyStamp = "cb54f60f-6282-4416-926c-d1edce822d07",
                        PhoneNumber = "0965679000",
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = true,
                        AccessFailedCount = 0,
                   },
                   new User()
                   {
                        Id = "c4a6876a-77fb-4e9e-9c78-a0880286aeV0",
                        CreatingTime = default,
                        LastLogin = default,
                        MiddleName = "MiddleName3",
                        FirstName = "FirstName3",
                        LastName = "LastName3",
                        UserName = "user3@gmail.com",
                        NormalizedUserName = "USER3@GMAIL.COM",
                        Email = "user3@gmail.com",
                        NormalizedEmail = "USER3@GMAIL.COM",
                        EmailConfirmed = false,
                        PasswordHash = "AQAAAAEAACcQAAAAEPXMPMbzuDZIKJUN4pMWRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                        SecurityStamp = "WGWJIYCCRG236HXFKGYS7H6QT2DE2LFF",
                        ConcurrencyStamp = "cb54f60f-70982-4416-926c-d1edce844d07",
                        PhoneNumber = "0675679312",
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = true,
                        AccessFailedCount = 0,
                   },
                   new User()
                   {
                        Id = "CEc4a6876a-77fb-4e9e-9c78-a0880286ae3c",
                        CreatingTime = default,
                        LastLogin = default,
                        MiddleName = "MiddleName4",
                        FirstName = "FirstName4",
                        LastName = "LastName4",
                        UserName = "user4@gmail.com",
                        NormalizedUserName = "USER4@GMAIL.COM",
                        Email = "user4@gmail.com",
                        NormalizedEmail = "USER4@GMAIL.COM",
                        EmailConfirmed = false,
                        PasswordHash = "AQAAAAEAACcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                        SecurityStamp = "WGWJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                        ConcurrencyStamp = "cb54f60f-6282-4416-926c-d1edce844d07",
                        PhoneNumber = "0965679312",
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = true,
                        AccessFailedCount = 0,
                   },
                   new User()
                   {
                        Id = "CVc4a6876a-77fb-4ecnne-9c78-a0880286ae3c",
                        CreatingTime = default,
                        LastLogin = default,
                        MiddleName = "MiddleName5",
                        FirstName = "FirstName5",
                        LastName = "LastName5",
                        UserName = "user5@gmail.com",
                        NormalizedUserName = "USER5@GMAIL.COM",
                        Email = "user5@gmail.com",
                        NormalizedEmail = "USER5@GMAIL.COM",
                        EmailConfirmed = false,
                        PasswordHash = "AQAAAAEAACcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                        SecurityStamp = "WGWJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                        ConcurrencyStamp = "cb54f60f-6282-4416-926c-d1edce844d07",
                        PhoneNumber = "0965889312",
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = true,
                        AccessFailedCount = 0,
                   },
                };

                context.Users.AddRangeAsync(users);
                context.SaveChangesAsync();
            }
        }
    }
}