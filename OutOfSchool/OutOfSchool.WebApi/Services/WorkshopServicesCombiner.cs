﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using Serilog;

namespace OutOfSchool.WebApi.Services
{
    public class WorkshopServicesCombiner : IWorkshopServicesCombiner
    {
        private readonly IWorkshopService databaseService;
        private readonly IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService;
        private readonly ILogger logger;
        private readonly IBackupTrackerService backupTrackerService;

        public WorkshopServicesCombiner(IWorkshopService workshopService, IElasticsearchService<WorkshopES, WorkshopFilterES> elasticsearchService, ILogger logger, IBackupTrackerService backupTrackerService)
        {
            this.databaseService = workshopService;
            this.elasticsearchService = elasticsearchService;
            this.logger = logger;
            this.backupTrackerService = backupTrackerService;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Create(WorkshopDTO dto)
        {
            var workshop = await databaseService.Create(dto).ConfigureAwait(false);

            //var esResultIsValid = await elasticsearchService.Index(workshop.ToESModel()).ConfigureAwait(false);

            var esResultIsValid = false;

            if (!esResultIsValid)
            {
                logger.Warning($"Error happend while trying to index {nameof(workshop)}:{workshop.Id} in Elasticsearch.");

                BackupTrackerDto backupTrackerDto = new BackupTrackerDto()
                {
                    Operation = OutOfSchool.Services.Enums.BackupOperation.Create,
                    OperationDate = DateTime.UtcNow,
                    TableName = nameof(workshop),
                    RecordId = workshop.Id,
                };
                var backupTracker = await backupTrackerService.Create(backupTrackerDto).ConfigureAwait(false);
            }

            return workshop;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> GetById(long id)
        {
            var workshop = await databaseService.GetById(id).ConfigureAwait(false);

            return workshop;
        }

        /// <inheritdoc/>
        public async Task<WorkshopDTO> Update(WorkshopDTO dto)
        {
            var workshop = await databaseService.Update(dto).ConfigureAwait(false);

            var esResultIsValid = await elasticsearchService.Update(workshop.ToESModel()).ConfigureAwait(false);

            if (!esResultIsValid)
            {
                logger.Warning($"Error happend while trying to update {nameof(workshop)}:{workshop.Id} in Elasticsearch.");
            }

            return workshop;
        }

        /// <inheritdoc/>
        public async Task Delete(long id)
        {
            await databaseService.Delete(id).ConfigureAwait(false);

            var esResultIsValid = await elasticsearchService.Delete(id).ConfigureAwait(false);

            if (!esResultIsValid)
            {
                logger.Warning($"Error happend while trying to delete Workshop:{id} in Elasticsearch.");
            }
        }

        /// <inheritdoc/>
        public async Task<SearchResult<WorkshopCard>> GetAll(OffsetFilter offsetFilter)
        {
            if (offsetFilter == null)
            {
                offsetFilter = new OffsetFilter();
            }

            var filter = new WorkshopFilter()
            {
                Size = offsetFilter.Size,
                From = offsetFilter.From,
                OrderByField = OrderBy.Id.ToString(),
            };

            var result = await elasticsearchService.Search(filter.ToESModel()).ConfigureAwait(false);

            if (result.TotalAmount > 0 || await elasticsearchService.PingServer().ConfigureAwait(false))
            {
                return result.ToSearchResult();
            }
            else
            {
                var databaseResult = await databaseService.GetByFilter(filter).ConfigureAwait(false);

                return new SearchResult<WorkshopCard>() { TotalAmount = databaseResult.TotalAmount, Entities = DtoModelsToWorkshopCards(databaseResult.Entities) };
            }
        }

        /// <inheritdoc/>
        public async Task<SearchResult<WorkshopCard>> GetByFilter(WorkshopFilter filter)
        {
            var result = await elasticsearchService.Search(filter.ToESModel()).ConfigureAwait(false);

            if (result.TotalAmount > 0 || await elasticsearchService.PingServer().ConfigureAwait(false))
            {
                return result.ToSearchResult();
            }
            else
            {
                var databaseResult = await databaseService.GetByFilter(filter).ConfigureAwait(false);

                return new SearchResult<WorkshopCard>() { TotalAmount = databaseResult.TotalAmount, Entities = DtoModelsToWorkshopCards(databaseResult.Entities) };
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<WorkshopDTO>> GetByProviderId(long id)
        {
            var workshop = await databaseService.GetByProviderId(id).ConfigureAwait(false);

            return workshop;
        }

        private List<WorkshopCard> DtoModelsToWorkshopCards(IEnumerable<WorkshopDTO> source)
        {
            List<WorkshopCard> workshopCards = new List<WorkshopCard>();
            foreach (var item in source)
            {
                workshopCards.Add(item.ToCardDto());
            }

            return workshopCards;
        }
    }
}
