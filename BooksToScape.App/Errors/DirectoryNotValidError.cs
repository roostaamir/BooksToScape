using FluentResults;

namespace BooksToScape.App.Errors;

public class DirectoryNotValidError : Error
{
    public DirectoryNotValidError(string directory) : base("Directory is not a valid one")
    {
    }
}