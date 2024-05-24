using Microsoft.AspNetCore.Mvc;
using System.Xml;
using System.Text;
using BookstoreAPI.Models;

namespace BookstoreAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly string _xmlFilePath;

        public BooksController()
        {
            _xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "bookstoreAPI.xml");
        }

        [HttpGet]
        public IActionResult GetBooks()
        {
            try
            {
                string xmlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "bookstoreAPI.xml");

                if (!System.IO.File.Exists(xmlFilePath))
                {
                    return NotFound("XML file not found.");
                }

                string xmlContent = System.IO.File.ReadAllText(xmlFilePath);

                return Ok(xmlContent);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("delete/{isbn}")]
        public IActionResult DeleteBook(string isbn)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(_xmlFilePath);
                XmlNode? bookNodeToDelete = xmlDoc.SelectSingleNode($"//book[isbn='{isbn}']");
                XmlNodeList? bookNodes = xmlDoc.SelectNodes("//book");

                if (bookNodeToDelete != null && bookNodes != null)
                {
                    if (bookNodeToDelete.ParentNode != null)
                    {
                        bookNodeToDelete.ParentNode.RemoveChild(bookNodeToDelete);
                        xmlDoc.Save(_xmlFilePath);
                        return Content(BuildReportHtml(bookNodes, "delete"), "text/html");
                    }
                }
                else
                {
                    return NotFound("Book not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            return StatusCode(500, "An unexpected error occurred.");
        }

        [HttpPost("add")]
        public IActionResult AddBook([FromBody] BookToAdd bookToAdd)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(_xmlFilePath);

                XmlElement bookNode = xmlDoc.CreateElement("book");
                XmlNodeList? bookNodes = xmlDoc.SelectNodes("//book");

                bookNode.Attributes.Append(xmlDoc.CreateAttribute("category")).Value = bookToAdd.Category ?? string.Empty;
                if (!string.IsNullOrEmpty(bookToAdd.Cover))
                {
                    bookNode.Attributes.Append(xmlDoc.CreateAttribute("cover")).Value = bookToAdd.Cover;
                }

                XmlElement isbnNode = xmlDoc.CreateElement("isbn");
                isbnNode.InnerText = bookToAdd.Isbn;
                bookNode.AppendChild(isbnNode);

                XmlElement titleNode = xmlDoc.CreateElement("title");
                titleNode.InnerText = bookToAdd.Title;
                if (!string.IsNullOrEmpty(bookToAdd.TitleLang))
                {
                    titleNode.Attributes.Append(xmlDoc.CreateAttribute("lang")).Value = bookToAdd.TitleLang;
                }
                bookNode.AppendChild(titleNode);

                foreach (string author in bookToAdd.Authors)
                {
                    XmlElement authorNode = xmlDoc.CreateElement("author");
                    authorNode.InnerText = author;
                    bookNode.AppendChild(authorNode);
                }

                XmlElement yearNode = xmlDoc.CreateElement("year");
                yearNode.InnerText = bookToAdd.Year.ToString();
                bookNode.AppendChild(yearNode);

                XmlElement priceNode = xmlDoc.CreateElement("price");
                priceNode.InnerText = bookToAdd.Price.ToString();
                bookNode.AppendChild(priceNode);

                xmlDoc.DocumentElement?.AppendChild(bookNode);
                xmlDoc.Save(_xmlFilePath);

                if (bookNodes != null)
                {
                    return Content(BuildReportHtml(bookNodes, "add"), "text/html");
                }

                return StatusCode(500, "Failed to add book to the XML document.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("edit/{isbn}")]
        public IActionResult EditBook(string isbn, [FromBody] BookToEdit updatedBook)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(_xmlFilePath);

                XmlNode? bookNode = xmlDoc.SelectSingleNode($"//book[isbn='{isbn}']");

                if (bookNode != null)
                {
                    XmlNodeList? bookNodes = xmlDoc.SelectNodes("//book");
                    XmlNodeList? childNodes = bookNode.ChildNodes;
                    XmlAttribute? categoryAttribute = bookNode.Attributes?["category"];
                    XmlAttribute? coverAttribute = bookNode.Attributes?["cover"];

                    if (categoryAttribute != null)
                    {
                        categoryAttribute.Value = updatedBook.Category ?? string.Empty;
                    }
                    else if (!string.IsNullOrEmpty(updatedBook.Category))
                    {
                        if (bookNode.OwnerDocument != null)
                        {
                            categoryAttribute = bookNode.OwnerDocument.CreateAttribute("category");
                            categoryAttribute.Value = updatedBook.Category;
                            bookNode.Attributes?.Append(categoryAttribute);
                        }
                    }

                    if (coverAttribute != null)
                    {
                        coverAttribute.Value = updatedBook.Cover ?? string.Empty;
                    }
                    else if (!string.IsNullOrEmpty(updatedBook.Cover))
                    {
                        if (bookNode.OwnerDocument != null)
                        {
                            coverAttribute = bookNode.OwnerDocument.CreateAttribute("cover");
                            coverAttribute.Value = updatedBook.Cover;
                            bookNode.Attributes?.Append(coverAttribute);
                        }
                    }

                    if (childNodes != null)
                    {
                        foreach (XmlNode childNode in childNodes)
                        {
                            switch (childNode.Name)
                            {
                                case "title":
                                    if (!string.IsNullOrEmpty(updatedBook.Title))
                                    {
                                        childNode.InnerText = updatedBook.Title;
                                    }
                                    if (!string.IsNullOrEmpty(updatedBook.TitleLang))
                                    {
                                        XmlAttribute? langAttr = childNode.Attributes?["lang"];
                                        if (langAttr == null)
                                        {
                                            langAttr = xmlDoc.CreateAttribute("lang");
                                            childNode.Attributes?.Append(langAttr);
                                        }
                                        langAttr.Value = updatedBook.TitleLang;
                                    }
                                    break;
                                case "author":
                                    if (updatedBook.Authors != null && updatedBook.Authors.Count > 0)
                                    {
                                        while (bookNode.SelectSingleNode("author") != null)
                                        {
                                            XmlNode? authorNode = bookNode.SelectSingleNode("author");
                                            if (authorNode != null)
                                            {
                                                bookNode.RemoveChild(authorNode);
                                            }
                                        }
                                        foreach (string author in updatedBook.Authors)
                                        {
                                            XmlElement authorNode = xmlDoc.CreateElement("author");
                                            authorNode.InnerText = author;
                                            bookNode.AppendChild(authorNode);
                                        }
                                    }
                                    break;
                                case "year":
                                    if (updatedBook.Year != 0)
                                    {
                                        childNode.InnerText = updatedBook.Year.ToString();
                                    }
                                    break;
                                case "price":
                                    if (updatedBook.Price != 0)
                                    {
                                        childNode.InnerText = updatedBook.Price.ToString();
                                    }
                                    break;
                            }
                        }
                    }

                    xmlDoc.Save(_xmlFilePath);

                    if (bookNodes != null)
                    {
                        return Content(BuildReportHtml(bookNodes, "edit"), "text/html");
                    }

                    return StatusCode(500, "Failed to edit book in the XML document.");
                }
                else
                {
                    return NotFound("Book not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private string BuildReportHtml(XmlNodeList bookNodes, string action)
        {
            StringBuilder htmlBuilder = new StringBuilder();

            htmlBuilder.Append("<html>");
            htmlBuilder.Append("<style>");
            htmlBuilder.Append("body { font-family: Arial, sans-serif; }");
            htmlBuilder.Append("table { width: 50%; margin: auto; border-collapse: collapse; }");
            htmlBuilder.Append("td { font-size: 20px; padding: 10px; }");
            htmlBuilder.Append("th { font-size: 24px; padding: 15px; }");
            htmlBuilder.Append("</style>");
            htmlBuilder.Append("<body>");
            htmlBuilder.Append(action switch
            {
                "delete" => "<h2>Book deleted successfully.</h2>",
                "edit" => "<h2>Book updated successfully.</h2>",
                "add" => "<h2>Book added successfully.</h2>",
                _ => throw new ArgumentOutOfRangeException(nameof(action), $"Not expected action value: {action}")
            });
            htmlBuilder.Append("<table>");
            htmlBuilder.Append("<tr><th>Title</th><th>Author(s)</th><th>Year</th><th>Price</th></tr>");

            if (bookNodes != null)
            {
                foreach (XmlNode bookNode in bookNodes)
                {
                    string title = bookNode["title"]?.InnerText ?? "N/A";
                    string authors = string.Join(", ", bookNode.SelectNodes("author")?.Cast<XmlNode>().Select(a => a.InnerText) ?? Enumerable.Empty<string>());
                    string year = bookNode["year"]?.InnerText ?? "N/A";
                    string price = bookNode["price"]?.InnerText ?? "N/A";

                    htmlBuilder.Append("<tr>");
                    htmlBuilder.Append($"<td>{title}</td>");
                    htmlBuilder.Append($"<td>{authors}</td>");
                    htmlBuilder.Append($"<td>{year}</td>");
                    htmlBuilder.Append($"<td>{price}</td>");
                    htmlBuilder.Append("</tr>");
                }
            }

            htmlBuilder.Append("</table>");
            htmlBuilder.Append("</body>");
            htmlBuilder.Append("</html>");

            return htmlBuilder.ToString();
        }
    }
}