# Bookstore API

The Bookstore API is a RESTful web service built using ASP.NET Core MVC for managing books in a bookstore. It allows users to perform CRUD (Create, Read, Update, Delete) operations on books stored in an XML file and returns HTML reports for each action.

## Features

- **Get Books**: Retrieve a list of all books in the bookstore.
- **Add Book**: Add a new book to the bookstore.
- **Edit Book**: Update an existing book in the bookstore.
- **Delete Book**: Remove a book from the bookstore.

## Prerequisites

Before running the application, ensure you have the following installed:

- [.NET Core SDK](https://dotnet.microsoft.com/download)

## Installation

1. Clone the repository:

```bash
git clone https://github.com/DanielAvishid/BookstoreAPI.git
```

2. Navigate to the project directory:

```bash
cd BookstoreAPI
```

3. Build the project:

```bash
dotnet build
```

4. Run the project:

```bash
dotnet run
```

The API will be accessible at `http://localhost:5000`. Additionally, you can navigate to [https://localhost:5001/](https://localhost:5001/) to access the API over HTTPS.
