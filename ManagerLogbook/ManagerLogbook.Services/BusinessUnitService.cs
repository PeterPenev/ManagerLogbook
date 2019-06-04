﻿using ManagerLogbook.Data;
using ManagerLogbook.Data.Models;
using ManagerLogbook.Services.Contracts;
using ManagerLogbook.Services.Contracts.Providers;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ManagerLogbook.Services.DTOs;
using ManagerLogbook.Services.Mappers;
using ManagerLogbook.Services.Utils;
using ManagerLogbook.Services.CustomExeptions;

namespace ManagerLogbook.Services
{
    public class BusinessUnitService : IBusinessUnitService
    {
        private readonly ManagerLogbookContext context;
        private readonly IBusinessValidator businessValidator;

        public BusinessUnitService(ManagerLogbookContext context,
                                   IBusinessValidator businessValidator)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.businessValidator = businessValidator ?? throw new ArgumentNullException(nameof(businessValidator));
        }

        public async Task<BusinessUnitDTO> CreateBusinnesUnitAsync(string brandName, string address, string phoneNumber, string email, string information, int businessUnitCategoryId, int townId)
        {
            businessValidator.IsNameInRange(brandName);
            businessValidator.IsAddressInRange(address);
            businessValidator.IsEmailValid(email);
            businessValidator.IsPhoneNumberValid(phoneNumber);
            businessValidator.IsDescriptionInRange(information);

            var businessUnit = new BusinessUnit() { Name = brandName, Address = address, PhoneNumber = phoneNumber, Email = email, Information = information, BusinessUnitCategoryId = businessUnitCategoryId, TownId = townId };

            this.context.BusinessUnits.Add(businessUnit);
            await this.context.SaveChangesAsync();

            var result = await this.context.BusinessUnits
                                           .Include(buc => buc.BusinessUnitCategory)
                                           .Include(t => t.Town)
                                           .FirstOrDefaultAsync(x => x.Id == businessUnit.Id);

            return result.ToDTO();
        }

        public async Task<BusinessUnitDTO> GetBusinessUnitById(int businessUnitId)
        {
            var result = await this.context.BusinessUnits
                                           .Include(buc => buc.BusinessUnitCategory)
                                           .Include(t => t.Town)
                                           .FirstOrDefaultAsync(x => x.Id == businessUnitId);

            if (result == null)
            {
                throw new NotFoundException(ServicesConstants.BusinessUnitNotFound);
            }

            return result.ToDTO();

        }

        public async Task<BusinessUnitDTO> UpdateBusinessUnitAsync(int businessUnitId, string brandName, string address, string phoneNumber, string information, string email, string picture)
        {
            var businessUnit = await this.context.BusinessUnits.FindAsync(businessUnitId);

            if (businessUnit == null)
            {
                throw new NotFoundException(ServicesConstants.BusinessUnitNotFound);
            }

            if (brandName != null)
            {
                businessValidator.IsNameInRange(brandName);
            }

            businessUnit.Name = brandName;

            if (address != null)
            {
                businessValidator.IsAddressInRange(address);
            }

            businessUnit.Address = address;

            if (phoneNumber != null)
            {
                businessValidator.IsPhoneNumberValid(phoneNumber);
            }

            businessUnit.PhoneNumber = phoneNumber;

            if (email != null)
            {
                businessValidator.IsEmailValid(email);
            }

            businessUnit.Email = email;

            if (information != null)
            {
                businessValidator.IsDescriptionInRange(information);
            }

            businessUnit.Information = information;

            if (picture != null)
            {
                businessUnit.Picture = picture;
            }

            await this.context.SaveChangesAsync();

            var result = await this.context.BusinessUnits
                                           .Include(buc => buc.BusinessUnitCategory)
                                           .Include(t => t.Town)
                                           .FirstOrDefaultAsync(x => x.Id == businessUnit.Id);

            return result.ToDTO();
        }

        public async Task<IReadOnlyCollection<LogbookDTO>> GetAllLogbooksForBusinessUnitAsync(int businessUnitId)
        {
            var businessUnit = await this.context.BusinessUnits.FindAsync(businessUnitId);

            if (businessUnit == null)
            {
                throw new NotFoundException(ServicesConstants.BusinessUnitNotFound);
            }

            var logbooksDTO = await this.context.Logbooks
                         .Include(n => n.Notes)
                         .Include(bu => bu.BusinessUnit)
                         .ThenInclude(t => t.Town)
                         .Where(bu => bu.BusinessUnitId == businessUnitId)
                         .Select(x => x.ToDTO())
                         .ToListAsync();

            return logbooksDTO;
        }

        public async Task<BusinessUnitCategoryDTO> CreateBusinessUnitCategoryAsync(string businessUnitCategoryName)
        {
            businessValidator.IsNameInRange(businessUnitCategoryName);

            var businessUnitCategory = new BusinessUnitCategory() { Name = businessUnitCategoryName };

            await this.context.SaveChangesAsync();

            return businessUnitCategory.ToDTO();
        }

        public async Task<BusinessUnitCategoryDTO> UpdateBusinessUnitCategoryAsync(int businessUnitCategoryId, string newBusinessUnitCategoryName)
        {
            businessValidator.IsNameInRange(newBusinessUnitCategoryName);

            var businessUnitCategory = await this.context.BusinessUnitCategories.FindAsync(businessUnitCategoryId);

            if (businessUnitCategory == null)
            {
                throw new NotFoundException(ServicesConstants.BusinessUnitCategoryNotFound);
            }

            businessUnitCategory.Name = newBusinessUnitCategoryName;

            await this.context.SaveChangesAsync();

            return businessUnitCategory.ToDTO();
        }

        public async Task<BusinessUnitDTO> AddBusinessUnitCategoryToBusinessUnitAsync(int businessUnitCategoryId, int businessUnitId)
        {
            var businessUnit = await this.context.BusinessUnits.FindAsync(businessUnitId);

            if (businessUnit == null)
            {
                throw new NotFoundException(ServicesConstants.BusinessUnitNotFound);
            }

            var businessUnitCategory = await this.context.BusinessUnitCategories.FindAsync(businessUnitCategoryId);

            if (businessUnitCategory == null)
            {
                throw new NotFoundException(ServicesConstants.BusinessUnitCategoryNotFound);
            }

            businessUnit.BusinessUnitCategoryId = businessUnitCategoryId;

            await this.context.SaveChangesAsync();

            var result = await this.context.BusinessUnits
                                           .Include(buc => buc.BusinessUnitCategory)
                                           .Include(t => t.Town)
                                           .FirstOrDefaultAsync(x => x.Id == businessUnit.Id);

            return result.ToDTO();
        }

        public async Task<BusinessUnitCategoryDTO> GetBusinessUnitCategoryByIdAsync(int businessUnitCategoryId)
        {
            var businessUnitCategory = await this.context.BusinessUnitCategories.FindAsync(businessUnitCategoryId);

            if (businessUnitCategory == null)
            {
                throw new NotFoundException(ServicesConstants.BusinessUnitCategoryNotFound);
            }

            return businessUnitCategory.ToDTO();
        }

        public async Task<IReadOnlyCollection<BusinessUnitDTO>> GetAllBusinessUnitsByCategoryIdAsync(int businessUnitCategoryId)
        {
            var businessUnitCategory = await this.context.BusinessUnitCategories.FindAsync(businessUnitCategoryId);

            if (businessUnitCategory == null)
            {
                throw new NotFoundException(ServicesConstants.BusinessUnitCategoryNotFound);
            }

            var businessUnits = await this.context.BusinessUnits
                         .Include(buc => buc.BusinessUnitCategory)
                         .Include(t => t.Town)
                         .Where(bc => bc.BusinessUnitCategoryId == businessUnitCategoryId)
                         .Select(bu => bu.ToDTO())
                         .ToListAsync();

            return businessUnits;
        }

        public async Task<IReadOnlyCollection<BusinessUnitDTO>> GetAllBusinessUnitsAsync()
        {
            var businessUnitsDTO = await this.context.BusinessUnits
                         .Include(buc => buc.BusinessUnitCategory)
                         .Include(t => t.Town)
                         .OrderByDescending(id => id.Id)
                         .Select(x => x.ToDTO())
                         .ToListAsync();

            return businessUnitsDTO;
        }

        public async Task<IReadOnlyCollection<Town>> GetAllTownsAsync()
        {
            var towns = await this.context.Towns
                                          .OrderByDescending(n => n.Name)
                                          .ToListAsync();

            return towns;
        }

        public async Task<BusinessUnitDTO> AddModeratorToBusinessUnitsAsync(string moderatorId, int businessUnitId)
        {
            var businessUnit = await this.context.BusinessUnits.FindAsync(businessUnitId);

            if (businessUnit == null)
            {
                throw new NotFoundException(ServicesConstants.BusinessUnitNotFound);
            }

            var moderatorUser = await this.context.Users.FindAsync(moderatorId);

            if (moderatorUser == null)
            {
                throw new NotFoundException(ServicesConstants.UserNotFound);
            }

            moderatorUser.BusinessUnitId = businessUnitId;

            businessUnit = await this.context.BusinessUnits
                         .Include(bc => bc.BusinessUnitCategory)
                         .Include(t => t.Town)
                         .FirstOrDefaultAsync(x => x.Id == businessUnitId);

            return businessUnit.ToDTO();
        }

        public async Task<IReadOnlyCollection<BusinessUnitDTO>> SearchBusinessUnitsAsync(string searchCriteria, int businessUnitCategoryId, int townId)
        {
            IQueryable<BusinessUnit> searchCollection = this.context.BusinessUnits.Where(n => n.Name.ToLower().Contains(searchCriteria.ToLower()));

            IQueryable<BusinessUnit> searchCategoryCollection = this.context.BusinessUnits.Where(buc => buc.BusinessUnitCategoryId == businessUnitCategoryId);

            IQueryable<BusinessUnit> searchTownCollection = this.context.BusinessUnits.Where(t => t.TownId == townId);

            var search = searchTownCollection.Intersect(searchCollection.Intersect(searchCategoryCollection));

            var businessUnitsDTO = await search.Include(t => t.Town)
                              .Include(buc => buc.BusinessUnitCategory)
                              .Select(x => x.ToDTO())
                              .ToListAsync();

            return businessUnitsDTO;
        }
    }
}
