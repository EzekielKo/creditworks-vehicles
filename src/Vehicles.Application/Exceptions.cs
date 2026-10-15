namespace Vehicles.Application;

public class NotFoundException(string message) : Exception(message);

public class ValidationFailedException(IReadOnlyList<string> errors)
    : Exception("Validation failed.")
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
