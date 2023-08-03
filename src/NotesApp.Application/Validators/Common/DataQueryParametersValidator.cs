using FluentValidation;
using NotesApp.Application.Common;
using NotesApp.Common;
using NotesApp.Common.Models;
using NotesApp.Domain.Entities;

namespace NotesApp.Application.Validators.Common
{
    public class DataQueryParametersValidator : AbstractValidator<DataQueryParameters>
    {
        public DataQueryParametersValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage(ResponseMessages.InvalidPageNumber);

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage(ResponseMessages.InvalidPageSize);

            RuleFor(x => x.SortOrder)
                .Must(s => s == Constants.Ascending || s == Constants.Descending)
                .WithMessage(ResponseMessages.InvalidSortOrder)
                .When(x => !string.IsNullOrEmpty(x.SortOrder));

            RuleFor(x => x)
                .Must(p => p.FilterColumns.Length == p.FilterQueries.Length)
                .WithMessage(ResponseMessages.InvalidFilterParameters);

            RuleFor(x => x.SortBy)
                .Must(IsValidColumnOfNote)
                .WithMessage(x => string.Format(ResponseMessages.InvalidSortByColumn, x.SortBy))
                .When(x => !string.IsNullOrEmpty(x.SortBy));

            RuleForEach(x => x.FilterColumns)
                .Must(IsValidColumnOfNote)
                .WithMessage((x, filterColumn) => string.Format(ResponseMessages.InvalidFilterColumn, filterColumn))
                .When(x => x.FilterColumns.Length > 0);

        }

        private bool IsValidColumnOfNote(string columnName)
        {
            var propertyNames = typeof(Note).GetProperties().Select(property => property.Name).ToList();
            return propertyNames.Contains(columnName);
        }
    }

}
