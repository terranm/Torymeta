using System.Collections.Generic;

/// <summary>
/// Base server response object.
/// </summary>
[System.Serializable]
public class RequestResponse
{
    // 비방 생성 roomId 추가 23. 06. 11
    public string roomId;
    public string rawResponse;
    /// <summary>
    /// Did the request result with an error?
    /// </summary>
    public bool error;
    /// <summary>
    /// Server response data. Will be an error message when <see cref="error"/> is true.
    /// </summary>
    public string output = "Some error occurred :(";
}

/// <summary>
/// Server response object when creating a new account or signing into an existing account.
/// </summary>
[System.Serializable]
public class UserAuthResponse : RequestResponse
{
    /// <summary>
    /// Response data that contains user data as well room seat reservation data.
    /// </summary>
    public new UserAuthData output;
}

[System.Serializable]
public class ChattingLoungeListResponse : RequestResponse
{
    public string resultCode;
    public string resultMessage;
    public new ChattingLoungeListResult result;
}

[System.Serializable]
public class ChattingLoungeListResult
{
    public List<RoomInfo> rooms;
}

[System.Serializable]
public class RoomInfo
{
    /*
                "clients": 0,
                "locked": false,
                "privateRoom": true,
                "maxClients": 6,
                "unlisted": false,
                "createdAt": "2023-06-21T02:24:57.013Z",
                "creatorName": "방장 닉네임",
                "roomId": "PuuVtjTK-",
                "password": "1234",
                "title": "chat title",
                "image": "http://127.0.0.1",
                "chatGroup": "chat_lounge",
                "tableId": "table1"
     */
    public int clients;
    public bool locked;
    public bool privateRoom;
    public int maxClients;
    public bool unlisted;
    public string createdAt;
    public string creatorName;
    public string roomId;
    public string password;
    public string title;
    public string image;
    public string chatGroup;
    public string tableId;
}
