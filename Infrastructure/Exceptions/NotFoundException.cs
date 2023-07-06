namespace Infrastructure.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string? message = null) : base(message) { }
}