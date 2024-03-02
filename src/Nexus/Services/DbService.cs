﻿using Microsoft.EntityFrameworkCore;
using Nexus.Core;

namespace Nexus.Services
{
    internal interface IDBService
    {
        IQueryable<NexusUser> GetUsers();
        Task<NexusUser?> FindUserAsync(string userId);
        Task<NexusClaim?> FindClaimAsync(Guid claimId);
        Task<RefreshToken?> FindRefreshTokenAsync(Guid tokenId, bool includeUserClaims);
        Task AddOrUpdateUserAsync(NexusUser user);
        Task AddOrUpdateClaimAsync(NexusClaim claim);
        Task AddOrUpdateRefreshTokenAsync(RefreshToken token);
        Task DeleteUserAsync(string userId);
        Task SaveChangesAsync();
    }

    internal class DbService : IDBService
    {
        private readonly UserDbContext _context;

        public DbService(
            UserDbContext context)
        {
            _context = context;
        }

        public IQueryable<NexusUser> GetUsers()
        {
            return _context.Users
                .Include(user => user.RefreshTokens);
        }

        public Task<NexusUser?> FindUserAsync(string userId)
        {
            return _context.Users
                .Include(user => user.Claims)
                .Include(user => user.RefreshTokens)
                .AsSingleQuery()
                .FirstOrDefaultAsync(user => user.Id == userId);

            /* .AsSingleQuery() avoids the following:
             *
             *   WRN: Microsoft.EntityFrameworkCore.Query
             *     Compiling a query which loads related collections for more 
             *     than one collection navigation, either via 'Include' or through 
             *     projection, but no 'QuerySplittingBehavior' has been configured. 
             *     By default, Entity Framework will use 'QuerySplittingBehavior.SingleQuery', 
             *     which can potentially result in slow query performance. See 
             *     https:*go.microsoft.com/fwlink/?linkid=2134277 for more information. 
             *     To identify the query that's triggering this warning call 
             *     'ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))'. 
             */

        }

        public async Task<NexusClaim?> FindClaimAsync(Guid claimId)
        {
            var claim = await _context.Claims
                .Include(claim => claim.Owner)
                .FirstOrDefaultAsync(claim => claim.Id == claimId);

            return claim;
        }

        public async Task<RefreshToken?> FindRefreshTokenAsync(Guid tokenId, bool includeUserClaims)
        {
            RefreshToken? token;

            var query = _context.RefreshTokens
                .Include(token => token.Owner);

            if (includeUserClaims)
                token = await _context.RefreshTokens
                    .Include(token => token.Owner)
                    .ThenInclude(owner => owner.Claims)
                    .FirstOrDefaultAsync(token => token.Id == tokenId);

            else
                token = await _context.RefreshTokens
                    .Include(token => token.Owner)
                    .FirstOrDefaultAsync(token => token.Id == tokenId);

            return token;
        }

        public async Task AddOrUpdateUserAsync(NexusUser user)
        {
            var reference = await _context.FindAsync<NexusUser>(user.Id);

            if (reference is null)
                _context.Add(user);

            else // https://stackoverflow.com/a/64094369
                _context.Entry(reference).CurrentValues.SetValues(user);

            await _context.SaveChangesAsync();
        }

        public async Task AddOrUpdateClaimAsync(NexusClaim claim)
        {
            var reference = await _context.FindAsync<NexusClaim>(claim.Id);

            if (reference is null)
                _context.Add(claim);

            else // https://stackoverflow.com/a/64094369
                _context.Entry(reference).CurrentValues.SetValues(claim);

            await _context.SaveChangesAsync();
        }

        public async Task AddOrUpdateRefreshTokenAsync(RefreshToken token)
        {
            var reference = await _context.FindAsync<RefreshToken>(token.Id);

            if (reference is null)
                _context.Add(token);

            else // https://stackoverflow.com/a/64094369
                _context.Entry(reference).CurrentValues.SetValues(token);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = await FindUserAsync(userId);

            if (user is not null)
                _context.Users.Remove(user);

            await _context.SaveChangesAsync();
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
