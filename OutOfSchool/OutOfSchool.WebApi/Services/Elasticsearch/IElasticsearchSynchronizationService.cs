﻿using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    public interface IElasticsearchSynchronizationService
    {
        Task<bool> Synchronize();

        Task<IEnumerable<BackupTrackerDto>> GetAll();

        Task<bool> Synchronize2();
    }
}
