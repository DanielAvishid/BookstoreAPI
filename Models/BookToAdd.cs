namespace BookstoreAPI.Models
{
    public class BookToAdd
    {
        public string? Category { get; set; }
        public string? Cover { get; set; }
        public string Isbn { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? TitleLang { get; set; }
        public List<string> Authors { get; set; } = new List<string>();
        public int Year { get; set; }
        public decimal Price { get; set; }
    }
}