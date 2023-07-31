﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using NotesApp.Application.Common;
using NotesApp.Application.Dto;
using NotesApp.Application.Services.Notes;
using NotesApp.Common;
using NotesApp.Common.Models;

namespace NotesApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NoteController : ControllerBase
    {

        private readonly INoteService _noteService;

        public NoteController(INoteService noteService)
        {
            _noteService = noteService;
        }

        [HttpGet("GetNotesForUser/{userId}")]
        public async Task<IActionResult> GetNotesForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId) || !ObjectId.TryParse(userId, out _))
            {
                return BadRequest(ResponseMessages.InvalidUserId);
            }
            var notes = await _noteService.GetNotesForUserAsync(userId);
            return Ok(notes);
        }

        [HttpPost("AddNote")]
        public async Task<IActionResult> AddNoteAsync(NoteDto note)
        {
            var addedNote = await _noteService.AddNoteAsync(note);
            return Ok(addedNote);
        }

        [HttpPut("UpdateNote")]
        public async Task<IActionResult> UpdateNoteAsync(NoteDto note)
        {
            var updatedNote = await _noteService.UpdateNoteAsync(note);
            return Ok(updatedNote);
        }

        [HttpDelete("DeleteNote/{noteId}")]
        public async Task<IActionResult> DeleteNoteAsync(string noteId)
        {
            if (string.IsNullOrEmpty(noteId) || !ObjectId.TryParse(noteId, out _))
            {
                return BadRequest(ResponseMessages.InvalidNoteId);
            }
            await _noteService.DeleteNoteAsync(noteId);
            return Ok();
        }


        [HttpGet("GetNotesForUserWithPagination/{userId}")]
        public async Task<IActionResult> GetNotesForUserAsync(string userId, [FromQuery] DataQueryParameters parameters)
        {
            if (parameters.PageNumber <= 0)
            {
                return BadRequest(ResponseMessages.InvalidPageNumber);
            }
            if (parameters.PageSize <= 0)
            {
                return BadRequest(ResponseMessages.InvalidPageSize);
            }
            if (string.IsNullOrEmpty(userId) || !ObjectId.TryParse(userId, out _))
            {
                return BadRequest(ResponseMessages.InvalidUserId);
            }
            if (!string.IsNullOrEmpty(parameters.SortOrder))
            {
                if (!(parameters.SortOrder == Constants.Ascending || parameters.SortOrder == Constants.Descending))
                {
                    return BadRequest(ResponseMessages.InvalidSortOrder);
                }
            }
            if (parameters.FilterColumns.Length != parameters.FilterQueries.Length)
            {
                return BadRequest(ResponseMessages.InvalidFilterParameters);
            }
            var notes = await _noteService.GetNotesForUserAsync(userId, parameters);
            return Ok(notes);
        }


    }
}
