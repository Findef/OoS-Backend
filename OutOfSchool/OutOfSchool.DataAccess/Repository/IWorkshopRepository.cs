using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IWorkshopRepository : IEntityRepository<Workshop>
    {
        /// <summary>
        /// Checks entity classId existence.
        /// </summary>
        /// <param name="id">Class id.</param>
        /// <returns>True if Class exists, otherwise false.</returns>
        bool ClassExists(long id);

        /// <summary>
        /// Get list of workshops for synchronization by operation.
        /// </summary>
        /// <param name="operation">Create/Update/Delete.</param>
        /// <returns>List of workshops for synchronization.</returns>
        Task<IEnumerable<Workshop>> GetListOfWorkshopsForSynchronizationByOperation(ElasticsearchSyncOperation operation);
    }
}
