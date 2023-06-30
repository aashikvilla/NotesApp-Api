using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Application.Services.Notes;
using NotesApp.Domain.Entities;

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
        public async Task<IActionResult> GetNotesForUserAsync(int userId)
        {
            var notes= await _noteService.GetNotesForUserAsync(userId);
            return Ok(notes);
        }

        [HttpPost("AddNote")]
        public async Task<IActionResult> AddNoteAsync(Note note)
        {
            var addedNote = await _noteService.AddNoteAsync(note);
            return Ok(addedNote);
        }

        [HttpPut("UpdateNote")]
        public async Task<IActionResult> UpdateNoteAsync(Note note)
        {
            var updatedNote = await _noteService.UpdateNoteAsync(note);
            return Ok(updatedNote);
        }

        [HttpDelete("DeleteNote/{noteId}")]
        public async Task<IActionResult> DeleteNoteAsync(int noteId)
        {
            await _noteService.DeleteNoteAsync(noteId);
            return Ok();
        }


    }
}
