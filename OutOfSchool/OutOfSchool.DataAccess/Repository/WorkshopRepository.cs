using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class WorkshopRepository : EntityRepository<Workshop>, IWorkshopRepository
    {
        private readonly OutOfSchoolDbContext db;

        public WorkshopRepository(OutOfSchoolDbContext dbContext)
         : base(dbContext)
        {
            db = dbContext;
        }

        /// <inheritdoc/>
        public new async Task Delete(Workshop entity)
        {
            if (entity.Applications?.Count > 0)
            {
                foreach (var app in entity.Applications)
                {
                    db.Entry(app).State = EntityState.Deleted;
                }
            }

            if (entity.Teachers?.Count > 0)
            {
                foreach (var teacher in entity.Teachers)
                {
                    db.Entry(teacher).State = EntityState.Deleted;
                }
            }

            db.Entry(entity).State = EntityState.Deleted;
            db.Entry(entity.Address).State = EntityState.Deleted;

            await db.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public bool ClassExists(long id) => db.Classes.Any(x => x.Id == id);

        /// <inheritdoc/>
        public async Task<IEnumerable<Workshop>> GetListOfWorkshopsForSynchronizationByOperation(ElasticsearchSyncOperation operation)
        {
            var elasticsearchSyncRecords = db.ElasticsearchSyncRecords;

            var resultMaxDates = from record in elasticsearchSyncRecords
                                 group record by record.RecordId into groupedRecords
                                 select new { RecordId = groupedRecords.Key, MaxOperationDate = groupedRecords.Max(r => r.OperationDate) };

            var result = from rMaxDate in resultMaxDates
                         join record in elasticsearchSyncRecords on new { rMaxDate.RecordId, OperationDate = rMaxDate.MaxOperationDate } equals new { record.RecordId, record.OperationDate } into leftJoin
                         from joinedRecord in leftJoin
                         where joinedRecord.Operation == operation
                         select new { joinedRecord.RecordId, joinedRecord.OperationDate, joinedRecord.Operation };

            return await db.Workshops.Join(result, workshop => workshop.Id, result => result.RecordId, (workshop, result) => workshop).ToListAsync();
        }
    }
}