﻿using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Workshop entity.
    /// </summary>
    public interface IWorkshopService : ICRUDService<WorkshopDTO>
    {
        /// <summary>
        /// Get all workshops by provider Id.
        /// </summary>
        /// <param name="id">Provider's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{WorkshopDTO}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<WorkshopDTO>> GetByProviderId(long id);

        /// <summary>
        /// Get entities from the database that match filter's parameters.
        /// </summary>
        /// <param name="filter">Filter with specified searching parameters.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the <see cref="SearchResult{WorkshopCard}"/> that contains found elements.</returns>
        Task<SearchResult<WorkshopDTO>> GetByFilter(WorkshopFilterDto filter = null);
    }
}