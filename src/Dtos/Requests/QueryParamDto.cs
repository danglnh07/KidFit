namespace KidFit.Dtos.Requests
{
    public class QueryParamDto
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        public string? OrderBy { get; set; }
        public bool? IsAsc { get; set; } = true;
    }
}
