﻿using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using Serilog;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class StatisticServiceTest
    {
        private StatisticService service;
        private IApplicationRepository applicationRepository;
        private IWorkshopRepository workshopRepository;
        private IEntityRepository<Category> categoryRepository;
        private OutOfSchoolDbContext context;
        private Mock<ILogger> logger;

        [SetUp]
        public void SetUp()
        {
            context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());
            applicationRepository = new ApplicationRepository(context);
            workshopRepository = new WorkshopRepository(context);
            categoryRepository = new EntityRepository<Category>(context);
            logger = new Mock<ILogger>();
            service = new StatisticService(applicationRepository, workshopRepository, categoryRepository, logger.Object);
        }

        [Test]
        public async Task GetPopularWorkshops_ShouldReturnWorkshops()
        {
            // Arrange
            var expected = new WorkshopDTO
            {
                Id = 1,
                Title = "w1",
                CategoryId = 1,
                Teachers = null,
            };

            // Act
            var result = await service.GetPopularWorkshops(2).ConfigureAwait(false);

            // Assert
            var workshopDtos = result.ToList();
            workshopDtos.Should().HaveCount(2);
            workshopDtos.Should().ContainEquivalentOf(expected);
        }

        [Test]
        public async Task GetPopularCategories_ShouldReturnCategories()
        {
            // Arrange
            var categories = context.Categories;

            var expected = new CategoryStatistic
            {
                Category = categories.First().ToModel(),
                WorkshopsCount = 2,
                ApplicationsCount = 2,
            };

            // Act
            var result = await service.GetPopularCategoriesV2(2).ConfigureAwait(false);

            // Assert
            var categoryStatistics = result.ToList();
            categoryStatistics.Should().HaveCount(2);
            categoryStatistics.Should().ContainEquivalentOf(expected);
        }
    }
}
