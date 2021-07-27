﻿using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// The interface for CRUD operations with workshops.
    /// </summary>
    public interface IWorkshopServicesCombiner : ICRUDService<WorkshopDTO>
    {
        /// <summary>
        /// Get all entities from the database.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the <see cref="IEnumerable{TEntity}"/> that contains found elements.</returns>
        new Task<SearchResult<WorkshopCard>> GetAll();

        /// <summary>
        /// Get all workshops with the specified provider's Id.
        /// </summary>
        /// <param name="id">Provider's key.</param>
        /// <returns>A <see cref="Task{WorkshopDTO}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{WorkshopDTO}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<WorkshopDTO>> GetByProviderId(long id);

        /// <summary>
        /// Get all entities that matches filter's parameters.
        /// </summary>
        /// <param name="filter">Entity that represents searching parameters.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{WorkshopES}"/> that contains elements that were found.</returns>
        Task<SearchResult<WorkshopCard>> GetByFilter(WorkshopFilterDto filter);
    }
}