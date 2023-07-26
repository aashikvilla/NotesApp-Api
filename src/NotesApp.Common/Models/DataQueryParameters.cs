namespace NotesApp.Common.Models
{
    public class DataQueryParameters
    {
        public int PageSize { get; set; } = 10;

        public int PageNumber { get; set; } = 1;

        public string? SearchTerm { get; set; } = string.Empty;

        public string? SortBy { get; set; } = string.Empty;

        public string? SortOrder { get; set; } = string.Empty;

        public string[] FilterColumns { get; set; } = Array.Empty<string>();

        public string[] FilterQueries { get; set; } = Array.Empty<string>();
    }
}
