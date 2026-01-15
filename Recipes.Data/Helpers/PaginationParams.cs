namespace Recipes.Data.Helpers
{
    public class PaginationParams
    {
        const int maxPageSize = 20;
        public int PageNumber { get; set; } = 1;
        private int _pageSize;
        public int PageSize
        {
            get
            {
                if (_pageSize == 0)
                {
                    _pageSize = maxPageSize;
                }
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize || value < 0) ? maxPageSize : value;
            }
        }
    }
}
