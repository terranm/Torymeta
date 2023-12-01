// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.23
// 

using Colyseus.Schema;

public partial class NetworkedEntityState : Schema {
	
	// 비방 생성 Websocket 파라미터 추가 23. 06. 11
	
	[Type(0, "string")]
	public string entityId = default(string);

	[Type(1, "string")]
	public string chatId = default(string);

	[Type(2, "number")]
	public float xPos = default(float);

	[Type(3, "number")]
	public float yPos = default(float);

	[Type(4, "number")]
	public float zPos = default(float);

	[Type(5, "number")]
	public float xRot = default(float);

	[Type(6, "number")]
	public float yRot = default(float);

	[Type(7, "number")]
	public float zRot = default(float);

	[Type(8, "number")]
	public float wRot = default(float);

	[Type(9, "ref", typeof(AvatarState))]
	public AvatarState avatar = new AvatarState();

	[Type(10, "number")]
	public float coins = default(float);

	[Type(11, "number")]
	public float timestamp = default(float);

	[Type(12, "string")]
	public string username = default(string);

	[Type(13, "string")]
	public string seat = default(string); //해당 아바타의 앉음 여부
	
	[Type(14, "number")]
	public float chatRoomHistoryId = default(float); // 단순 서버 자장용 파라미터 
	
	[Type(15, "number")]
	public float memberId = default(float); // 단순 서버 자장용 파라미터 
	
	[Type(16, "boolean")]
	public bool roomMaker = default(bool);
	
	[Type(17, "number")]
	public float inputting = default(float); // 입력중일때 1로 변환하고, 다른 사용자가 1일때 입력상태 출력 submit일 때

	[Type(18, "string")]
	public string table = default(string); //해당 아바타의 앉음 여부
	
	[Type(19, "number")]
	public float clients = default(float); //접속된 룸의 현재 인원수
	
	[Type(20, "number")]
	public float maxClients = default(float); //접속된 룸의 최대 접속자 수
	
	[Type(21, "string")]
	public string password = default(string);
}
