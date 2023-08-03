using FluentValidation;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IValidator<DataQueryParameters> _dataQueryParametersValidator;
        private readonly IValidator<NoteDto> _noteDtoValidator;


        public NoteController(INoteService noteService, IValidator<NoteDto> noteDtoValidator, IValidator<DataQueryParameters> dataqueryParameterValidator)
        {
            _noteService = noteService;
            _noteDtoValidator = noteDtoValidator;
            _dataQueryParametersValidator = dataqueryParameterValidator;
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
            var validationResult = _noteDtoValidator.Validate(note, options => options.IncludeRuleSets(Constants.NoteDtoAddRuleSet, Constants.Default));

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var addedNote = await _noteService.AddNoteAsync(note);
            return Ok(addedNote);
        }

        [HttpPut("UpdateNote")]
        public async Task<IActionResult> UpdateNoteAsync(NoteDto note)
        {
            var validationResult = _noteDtoValidator.Validate(note, options => options.IncludeRuleSets(Constants.NoteDtoUpdateRuleSet, Constants.Default));

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
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
            if (string.IsNullOrEmpty(userId) || !ObjectId.TryParse(userId, out _))
            {
                return BadRequest(ResponseMessages.InvalidUserId);
            }

            var validationResult = _dataQueryParametersValidator.Validate(parameters);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var notes = await _noteService.GetNotesForUserAsync(userId, parameters);
            return Ok(notes);
        }


    }
}
