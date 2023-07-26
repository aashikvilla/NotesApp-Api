using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using NotesApp.Common.Models;
using NotesApp.Common;
using NotesApp.Domain.Entities;
using NotesApp.Domain.RepositoryInterfaces;
using NotesApp.Infrastructure.Data;

namespace NotesApp.Infrastructure.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly IMongoCollection<Note> _notes;
        public NoteRepository(IMongoDatabase mongoDatabase, IOptions<MongoDbSettings> mongoDbSettings)
        {
            _notes = mongoDatabase.GetCollection<Note>(
                mongoDbSettings.Value.NotesCollectionName);
        }

        public async Task<IEnumerable<Note>> GetNotesForUserAsync(string userId)
        {
            var filter = Builders<Note>.Filter.Eq(note => note.UserId, userId);
            return await _notes.Find(filter).ToListAsync();

        }

        public async Task<Note> GetNoteByIdAsync(string noteId)
        {
            var filter = Builders<Note>.Filter.Eq(note => note.Id, noteId);
            return await _notes.Find(filter).FirstOrDefaultAsync();
        }

        public async Task AddNoteAsync(Note note)
        {
            await _notes.InsertOneAsync(note);
        }

        public async Task UpdateNoteAsync(Note note)
        {
            var filter = Builders<Note>.Filter.Eq(n => n.Id, note.Id);
            await _notes.ReplaceOneAsync(filter, note);
        }

        public async Task DeleteNoteAsync(Note note)
        {
            var filter = Builders<Note>.Filter.Eq(n => n.Id, note.Id);
            await _notes.DeleteOneAsync(filter);
        }

        private FilterDefinition<Note> GetFilterByUserIdAndDataQueryParameters(string userId, DataQueryParameters parameters)
        {
            var filter = Builders<Note>.Filter.Eq(note => note.UserId, userId);
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                var searchFilter = Builders<Note>.Filter.Regex(nameof(Note.Title), new BsonRegularExpression(parameters.SearchTerm, "i")) |
                    Builders<Note>.Filter.Regex(nameof(Note.Description), new BsonRegularExpression(parameters.SearchTerm, "i")) |
                    Builders<Note>.Filter.Regex(nameof(Note.Status), new BsonRegularExpression(parameters.SearchTerm, "i")) |
                    Builders<Note>.Filter.Regex(nameof(Note.Priority), new BsonRegularExpression(parameters.SearchTerm, "i"));

                filter = filter & searchFilter;
            }

            if (parameters.FilterColumns.Length > 0)
            {
                for (int i = 0; i < parameters.FilterColumns.Length; i++)
                {
                    var columnFilter = Builders<Note>.Filter.Regex( parameters.FilterColumns[i], new BsonRegularExpression(parameters.FilterQueries[i], "i"));
                    filter = filter & columnFilter;
                }
            }

            return filter;
        }

     
        public async Task<PaginationResult<Note>> GetNotesForUserAsync(string userId, DataQueryParameters parameters)
        {
            var filter = GetFilterByUserIdAndDataQueryParameters(userId, parameters);

            var aggregate = _notes.Aggregate().Match(filter);

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                var sortCondition = (parameters.SortOrder.ToLower() == Constants.Descending) ? 
                    Builders<Note>.Sort.Descending(parameters.SortBy) : Builders<Note>.Sort.Ascending(parameters.SortBy);
                aggregate = aggregate.Sort(sortCondition);                
            }

            var notes = await aggregate
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Limit(parameters.PageSize)
                .ToListAsync();

            return new PaginationResult<Note>
            {
                Data = notes,
                Count = await _notes.CountDocumentsAsync(filter)
            };
        }

      
    }
}
