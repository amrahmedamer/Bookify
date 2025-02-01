using Bookify.Application.Common.Dto;

namespace Bookify.Extensions
{
    public static class FormCollectionExtensions
    {
        public static GetFilteredDto GetFilters(this IFormCollection Form)
        {
            var sorColumnIndex = Form["order[0][column]"];
            return new GetFilteredDto(
                searchValue: Form["search[value]"]!,
                sorColumn: Form[$"columns[{sorColumnIndex}][name]"]!,
                sorColumnDirection: Form["order[0][dir]"]!,
                skip: int.Parse(Form["start"]!),
                sizePage: int.Parse(Form["length"]!)
                );
        }
    }
}
