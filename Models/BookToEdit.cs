namespace BookstoreAPI.Models
{
    public class BookToEdit
    {
        public string? Category { get; set; }
        public string? Cover { get; set; }
        public string? Title { get; set; }
        public string? TitleLang { get; set; }
        public List<string>? Authors { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }
    }
}