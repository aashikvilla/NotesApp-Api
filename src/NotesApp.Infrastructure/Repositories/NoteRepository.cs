﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotesApp.Domain.Entities;
using NotesApp.Domain.RepositoryInterfaces;
using NotesApp.Infrastructure.Data;

namespace NotesApp.Infrastructure.Repositories
{
    public class NoteRepository:INoteRepository
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
    }
}
