﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Services
{
    public interface ICRUDService<TEntity>
        where TEntity : class, new()
    {
        /// <summary>
        /// Add entity to the database.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>A <see cref="Task{TEntity}"/> representing the result of the asynchronous operation.
        /// The task result contains the entity that was created.</returns>
        Task<TEntity> Create(TEntity dto);

        /// <summary>
        /// Get all entities from the database.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the <see cref="IEnumerable{TEntity}"/> that contains found elements.</returns>
        Task<IEnumerable<TEntity>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task{TEntity}"/> representing the result of the asynchronous operation.
        /// The task result contains the entity that was found, or null.</returns>
        Task<TEntity> GetById(long id);

        /// <summary>
        /// Update existing entity in the database.
        /// </summary>
        /// <param name="dto">Entity that will be to updated.</param>
        /// <returns>A <see cref="Task{TEntity}"/> representing the result of the asynchronous operation.
        /// The task result contains the entity that was updated.</returns>
        Task<TEntity> Update(TEntity dto);

        /// <summary>
        ///  Delete entity.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);
    }
}