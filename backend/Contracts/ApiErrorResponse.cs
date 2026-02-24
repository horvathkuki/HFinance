namespace backend.Contracts;

public record ApiErrorResponse(
    string Code,
    string Message,
    string TraceId);
