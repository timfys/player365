namespace GoldCasino.ApiModule.Common;

public class Result<T, E>
{
	public bool IsSuccess { get; }
	public T? Value { get; }
	public E? Error { get; }

	private Result(T value)
	{
		IsSuccess = true;
		Value = value;
		Error = default;
	}

	private Result(E error)
	{
		IsSuccess = false;
		Error = error;
		Value = default;
	}

	public static Result<T, E> Ok(T value) => new(value);
	public static Result<T, E> Fail(E error) => new(error);
}