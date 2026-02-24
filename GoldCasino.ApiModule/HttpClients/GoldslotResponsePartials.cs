namespace PalaceCasino.Agent.Client;

public interface IGoldSlotResponse<T>
{
	int Code { get; }
	string Message { get; }
	T Data { get; }
}

public partial class _UserCreateResultData
		: IGoldSlotResponse<_UserCreate>
{
	int IGoldSlotResponse<_UserCreate>.Code => (int)Code;
	string IGoldSlotResponse<_UserCreate>.Message => Message;
	_UserCreate IGoldSlotResponse<_UserCreate>.Data => Data;
}
