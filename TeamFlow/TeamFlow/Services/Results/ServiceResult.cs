namespace TeamFlow.Services.Results
{
    public class ServiceResult<T>
    {
        public bool Succeeded { get; init; }
        public string? Error { get; init; }
        public T? Data { get; init; }

        public static ServiceResult<T> Success(T? data = default)
            => new() { Succeeded = true, Data = data };

        public static ServiceResult<T> Failure(string error)
            => new() { Succeeded = false, Error = error };
    }
}
