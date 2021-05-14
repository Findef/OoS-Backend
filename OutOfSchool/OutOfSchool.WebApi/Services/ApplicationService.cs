﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Implements the interface with CRUD functionality for Application entity.
    /// </summary>
    public class ApplicationService : IApplicationService
    {
        private readonly IEntityRepository<Application> repository;
        private readonly ILogger logger;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationService"/> class.
        /// </summary>
        /// <param name="repository">Application repository.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="localizer">Localizer.</param>
        public ApplicationService(IEntityRepository<Application> repository, ILogger logger, IStringLocalizer<SharedResource> localizer)
        {
            this.repository = repository;
            this.logger = logger;
            this.localizer = localizer;
        }

        /// <inheritdoc/>
        public async Task<ApplicationDto> Create(ApplicationDto applicationDto)
        {
            logger.Information("Application creating was started.");

            var application = applicationDto.ToDomain();

            var newApplication = await repository.Create(application).ConfigureAwait(false);

            logger.Information("Application created succesfully.");

            return newApplication.ToModel();
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            logger.Information("Application delete was launching.");

            var application = new Application { Id = id };

            try
            {
                await repository.Delete(application).ConfigureAwait(false);

                logger.Information("Application successfully deleted.");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Deleting failed. There is no Application in the Db with such an id.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationDto>> GetAll()
        {
            logger.Information("Process of getting all Applications started.");

            var applications = await repository.GetAll().ConfigureAwait(false);

            logger.Information(!applications.Any()
                ? "Application table is empty."
                : "Successfully got all records from the Application table.");

            return applications.Select(a => a.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationDto>> GetAllByUser(string id)
        {
            logger.Information("Process of getting Application by User Id started.");

            Expression<Func<Application, bool>> filter = a => a.UserId == id;

            var applications = await repository.GetByFilter(filter).ConfigureAwait(false);

            if (!applications.Any())
            {
                throw new ArgumentException(localizer["There is no Application in the Db with such User id"], nameof(id));
            }

            logger.Information($"Successfully got Applications with User id = {id}.");

            return applications.Select(a => a.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ApplicationDto>> GetAllByWorkshop(long id)
        {
            logger.Information("Process of getting Application by Workshop Id started.");

            Expression<Func<Application, bool>> filter = a => a.WorkshopId == id;

            var applications = await repository.GetByFilter(filter).ConfigureAwait(false);

            if (!applications.Any())
            {
                throw new ArgumentException(localizer["There is no Application in the Db with such User id"], nameof(id));
            }

            logger.Information($"Successfully got Applications with Workshop id = {id}.");

            return applications.Select(a => a.ToModel()).ToList();
        }

        /// <inheritdoc/>
        public async Task<ApplicationDto> GetById(long id)
        {
            logger.Information("Process of getting Application by id started.");

            var application = await repository.GetById(id).ConfigureAwait(false);

            if (application is null)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be greater than number of table entities."]);
            }

            logger.Information($"Successfully got an Application with id = { id}.");

            return application.ToModel();
        }

        /// <inheritdoc/>
        public async Task<ApplicationDto> Update(ApplicationDto applicationDto)
        {
            logger.Information("Application updating was launched.");

            try
            {
                var application = await repository.Update(applicationDto.ToDomain()).ConfigureAwait(false);

                logger.Information("Application successfully updated.");

                return application.ToModel();
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.Error("Updating failed.There is no application in the Db with such an id.");
                throw;
            }
        }
    }
}
