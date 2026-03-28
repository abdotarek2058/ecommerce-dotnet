namespace IMDB.Core.ViewModel
{
    public class PaginatedResult<T>
    {
        public List<T> Data { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }
}
