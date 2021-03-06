using System;
using System.Collections.Generic;
using System.Linq;
using whale_spotting.Models.Database;
using whale_spotting.Models.Request;
using whale_spotting.Request;

namespace whale_spotting.Repositories
{
    public interface ISightingRepo
    {
        Sighting Submit(SubmitSightingRequest create);
        List<Sighting> GetRecentSightings();
        IEnumerable<Sighting> GetByConfirmState();
        void AddNewSightings(List<Sighting> sightingsToAdd);
        Sighting SelectSightingById(int Id);
        IEnumerable<Sighting> Search(SightingSearchRequest searchRequest);
        int Count(SightingSearchRequest search);
        Sighting ConfirmSighting(Sighting SightingToConfirm);
        Sighting UpdateAndConfirmSighting(Sighting SightingToUpdate);
        Sighting DeleteSighting(Sighting sighting);
        Sighting SelectLatestApiSighting();
        Sighting RestoreSighting(Sighting SightingToRestore);
    }

    public class SightingRepo : ISightingRepo
    {
        private readonly WhaleSpottingContext _context;

        public SightingRepo(WhaleSpottingContext context)
        {
            _context = context;
        }

        public Sighting Submit(SubmitSightingRequest create)
        {
            var insertResponse =
                _context
                    .Sightings
                    .Add(new Sighting
                    {
                        Species = create.Species,
                        Quantity = create.Quantity,
                        Location = create.Location,
                        Latitude = create.Latitude,
                        Longitude = create.Longitude,
                        Description = create.Description,
                        SightedAt = create.SightedAt,
                        SubmittedByName = create.SubmittedByName,
                        SubmittedByEmail = create.SubmittedByEmail,
                        CreatedAt = DateTime.Now
                    });
            _context.SaveChanges();

            return insertResponse.Entity;
        }

        public IEnumerable<Sighting> GetByConfirmState()
        {
            return _context
                .Sightings
                .Where(s => s.ConfirmState == ConfirmState.Review);
        }

        public void AddNewSightings(List<Sighting> sightingsToAdd)
        {
            var newSightingIds =
                sightingsToAdd.Select(s => s.ApiId).Distinct().ToArray();
            var SightingsInDb =
                _context
                    .Sightings
                    .Where(s => newSightingIds.Contains(s.ApiId))
                    .Select(s => s.ApiId)
                    .ToArray();
            var SightingsNotInDb =
                sightingsToAdd.Where(s => !SightingsInDb.Contains(s.ApiId));
            SightingsNotInDb
                .ToList()
                .ForEach(x => x.ConfirmState = ConfirmState.Confirmed);
            _context.Sightings.AddRange(SightingsNotInDb.ToArray());
            _context.SaveChanges();
        }

        public IEnumerable<Sighting> Search(SightingSearchRequest searchRequest)
        {
            IQueryable<Sighting> query = _context.Sightings;

            if (!string.IsNullOrEmpty(searchRequest.Species))
            {
                query =
                    query
                        .Where(s =>
                            s
                                .Species
                                .ToLower()
                                .Contains(searchRequest.Species.ToLower()));
            }
            if (searchRequest.SightedAt.HasValue)
            {
                query =
                    query
                        .Where(s =>
                            s.SightedAt >= searchRequest.SightedAt.Value &&
                            s.SightedAt <
                            searchRequest.SightedAt.Value.AddDays(1));
            }
            if (!string.IsNullOrEmpty(searchRequest.Location))
            {
                query =
                    query
                        .Where(s =>
                            s
                                .Location
                                .ToLower()
                                .Contains(searchRequest.Location.ToLower()));
            }
            return query
                .Where(s => s.ConfirmState == ConfirmState.Confirmed)
                .OrderByDescending(s => s.SightedAt)
                .Skip((searchRequest.Page - 1) * searchRequest.PageSize)
                .Take(searchRequest.PageSize);
        }

        public int Count(SightingSearchRequest searchRequest)
        {
            IQueryable<Sighting> query = _context.Sightings;
            if (!string.IsNullOrEmpty(searchRequest.Species))
            {
                query =
                    query
                        .Where(s =>
                            s
                                .Species
                                .ToLower()
                                .Contains(searchRequest.Species.ToLower()));
            }
            if (searchRequest.SightedAt.HasValue)
            {
                query =
                    query
                        .Where(s =>
                            s.SightedAt >= searchRequest.SightedAt.Value &&
                            s.SightedAt <
                            searchRequest.SightedAt.Value.AddDays(1));
            }
            if (!string.IsNullOrEmpty(searchRequest.Location))
            {
                query =
                    query
                        .Where(s =>
                            s
                                .Location
                                .ToLower()
                                .Contains(searchRequest.Location.ToLower()));
            }
            return query.Count();
        }

        public Sighting SelectSightingById(int Id)
        {
            var sighting =
                _context.Sightings.Where(s => s.Id == Id).SingleOrDefault();
            return sighting;
        }

        public List<Sighting> GetRecentSightings()
        {
            var sightingList = _context.Sightings.Where(s => s.ConfirmState == ConfirmState.Confirmed)
                                                 .OrderByDescending(x => x.SightedAt)
                                                 .Take(5)
                                                 .ToList();
            return sightingList;
        }
        
        public Sighting ConfirmSighting(Sighting SightingToConfirm)
        {
            SightingToConfirm.ConfirmState = ConfirmState.Confirmed;
            var ConfirmedSighting = _context.Update<Sighting>(SightingToConfirm);
            _context.SaveChanges();
            return ConfirmedSighting.Entity;
        }        

        public Sighting UpdateAndConfirmSighting(Sighting SightingToUpdate)
        {
            var UpdatedSighting = _context.Update<Sighting>(SightingToUpdate);
            _context.SaveChanges();
            return UpdatedSighting.Entity;
        }

        public Sighting DeleteSighting(Sighting sighting)
        {
            sighting.ConfirmState = ConfirmState.Deleted;
            var sightingDeleted = _context.Update<Sighting>(sighting);
            _context.SaveChanges();
            return sightingDeleted.Entity;
        }

        public Sighting SelectLatestApiSighting()
        {
            var sighting =
                _context.Sightings
                .OrderByDescending(x => x.CreatedAt)
                .Where(s => s.ApiId != null)
                .FirstOrDefault();
            return sighting;
        }

        public Sighting RestoreSighting(Sighting SightingToRestore)
        {
            SightingToRestore.ConfirmState = ConfirmState.Review;
            var sightingRestored = _context.Update<Sighting>(SightingToRestore);
            _context.SaveChanges();
            return sightingRestored.Entity;
        }
    }
}
