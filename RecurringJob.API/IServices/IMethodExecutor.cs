namespace RecurringJob.API.IServices;

public interface IMethodExecutor
{
    Task<object?> ExecuteMethodAsync(string methodName, string? parameters);
}
