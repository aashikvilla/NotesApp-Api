namespace NotesApp.Common.Models
{
    public class PaginationResult<T> where T : class
    {
        public List<T> Data { get; set; }
        public long Count { get; set; }
    }
}
