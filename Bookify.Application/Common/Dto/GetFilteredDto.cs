namespace Bookify.Application.Common.Dto
{
    public record GetFilteredDto(string searchValue, string sorColumn,string sorColumnDirection, int skip,int sizePage);
}
