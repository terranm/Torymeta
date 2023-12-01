using System;
using System.Collections.Generic;
using Colyseus.Schema;

public class ActionState : Schema
{
    [Colyseus.Schema.Type(0, "string")]
    public string entityId = default;
    [Colyseus.Schema.Type(1, "string")]
    public string actionId = default;
}

[Serializable]
public class sendPerm
{
    public int memberId;
}

[Serializable]
public class sendKick
{
    public int memberId;
}

[Serializable]
public class kick
{
    public string status;
}

[Serializable]
public class sendChangeRoomAttr
{
    /*
        “title”:”xxxxxxxx”,
        “description”:”xxxxxxxx”,
        “privateRoom”:0 or 1 
     */
    public string title;
    public string description;
    public int privateRoom;
}

[Serializable]
public class sendPenalty
{
    public int memberId;
    public int sec;
    public string penaltyType;
}

[Serializable]
public class penalty
{
    public int sec;
    public string penaltyType;
}


[Serializable]
public class ReceiveInvite
{
/*구분자
    inviterImgUrl: "https://test-torymeta-member.s3.ap-northeast-2.amazonaws.com/12_18B444BB-1376-42BA-88D3-DB56D25EEDBD.jpeg"
    inviterNick: "Tory"
    password: "aaaa"
    privateRoom: true
    roomId: "iUKHkRGuR"
    status: "ONLINE"
    title: "방제목5"
*/
    public string status;
    public string title;
    public string roomId;
    public string password;
    public bool privateRoom;
    public string inviterNick;
    public string inviterImgUrl;
}

[Serializable]
public class FriendList
{
    public string status;
    public List<FriendInfo> friendList;
    public int chatRoomId;
}

[Serializable]
public class FriendInfo
{
    /*
     {memberId: 25, profileName: '프랭크버거', phoneNum: '01050323572', imgUrl: 'https://test-torymeta-image.s3.ap-northeast-2.amazonaws.com/basic-profile-image/basic_profile_7.png'}  
     */
    public int memberId;
    public string profileName;
    public string phoneNum;
    public string imgUrl;
    public bool roomMaker;
}

[Serializable]
public class InviteFriendInfo
{
    public List<int> friendIds;
}

[Serializable]
public class InviteResult
{
    public string status;
}