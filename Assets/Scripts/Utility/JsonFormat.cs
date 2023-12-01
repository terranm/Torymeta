using System;
using System.Collections.Generic;

//23.07.14 리팩토링 작성 문서 기반 json form 추가 https://docs.google.com/document/d/1ZPIPVNyUNAJVhrdRBIJaBQWIjnhCCXeuAavZJInWLJk/edit
//기존 작성된 포멧은 Origin body region에 새로 작성된 포멧은 After Refactorying region에 분리 

#region Origin body

[Serializable]
public class JsonFormat
{
    public string avatarName { get; set; }
    public string avatarNumber { get; set; }
    public string sceneName { get; set; }
}

[Serializable]
public class SwitchScene
{
    //{\"member\":{ \"birth\":\"19940809\",\"emailId\":\"testaos@torymeta.io\",\"imgUrl\":\"https://test-torymeta-member.s3.ap-northeast-2.amazonaws.com/229412182654_img_230213_161149.jpg\",\"locationExposeYn\":1,\"memberId\":229412182654,\"name\":\"TempName\",\"temp\",\"phoneNum\":\"01021976830\",\"profileName\":\"Test\",\"universityCode\":\"CAU\"},\"scene\":\"university\",\"url\":\"www.youtube.com\"}

    public Member member;
    public Option options;
    public string scene;
    public string characterId;
}

[Serializable]
public class Member
{
    public string profileName;
    public int memberId;
    public string universityCode;
}

[Serializable]
public class Option
{
    public string roomType;
    public string roomId;
    public string title;
    public string url;
    public string videoStartTime;
    public bool isAppRelease;
}

[Serializable]
public class CharacterSelect
{
    public string characterId;
    public string item;
    public string color;
}

[Serializable]
public class Options
{
    public string roomId;
    public string videoVisibility;
    public string audioMute;
    public string textVisibility;
    public string openParticipation;
    public string roomOwner;
    public string url;

    // "roomId": "CAU_Seminar15",
    // "videoVisibility": "Y",
    // "audioMute": "N",
    // "textVisibility": "Y",
    // "openParticipation": "Y",
    // "roomOwner": "owner Name",
    // "url": "www.youtube.com"
}

[Serializable]
public class ChatLoungeCreateData
{
    /*
     roomname - 	방이름 문자열
	isPrivate - 비공개방 여부(비공개 방일때 true 인 bool)
	password - 암호 문자열
     */
    public string roomName;
    public bool isPrivate;
    public string password;
}

[Serializable]
public class ChatLoungeInviteData
{
    /*
     memberId : N //초대할 사용자의 memberId 정수
     */
    public int memberId;
}

[Serializable]
public class ChatLoungeJoinData
{
    /*
       tableId : “tableId 문자열”
     */
    public string tableId;
}

[Serializable]
public class LoungeVideoUrl
{
    /*
       url : “url 문자열”
     */
    public string url;
}


[Serializable]
public class BasicModalType
{
    public string title;
    public string description;
    public List<BasiBbuttonType> actions;
}

[Serializable]
public class BasiBbuttonType
{
    public string title;
    public string color;
    public string actionId;
}

[Serializable]
public class IOSMessageType
{
    public string functionName;
    public BasicModalType value;
}
    
    
    
[Serializable]
public class AtionType
{
    public string actionId;
}

#endregion

//---------------------------------------region divide line -------------------------------------------

#region After Refactorying

[Serializable]
public class BasicMessage
{
    /*
        type : “문자열”
        value : Json
     */
    public string type;
    public Object value;
}

[Serializable]
public class ConfirmForm
{
    /*
            title:”타이틀이 되어야 할 문자열”
			description : “내용이 될 문자열”
			buttons :
                       [
				{
					title :”버튼에 표기 될 문자열”
					function : ”유니티에 호출 함수 이름  빈 값의 경우 동작 없이 창 닫기”
				},
				{
					title :” 두번째 버튼에 표기 될 문자열”
					function : ”유니티에 호출 함수 이름  빈 값의 경우 동작 없이 창 닫기”
				}
  			 ]

     */
    public string title;
    public string description;
    public List<ButtonForm> buttons;
}

[Serializable]
public class ButtonForm
{
    /*
			title :”버튼에 표기 될 문자열”
			color : “#000000” // 버튼 글자색
			actionId : ”유니티에서 행위를 정의한 ActionID”
     */
    public string title;
    public string color;
    public string actionId;

}

[Serializable]
public class FixedFormCall
{
    /*
			form:”폼 종류 문자열 아래 참조”
			JsonForm// 아래 참조
			
채팅 라운지 목록 출력                    ChatList
채팅 생성 출력                        ChatCreate
비공개방 입장 암호 입력창 출력            PasswordInput               PwInputOptionForm
참석자 목록 출력                       ParticipantsList            ParticipantsListOptionForm
초대가능 목록 출력                  InvitableList                   UserList
방장/참석자 권한 메뉴 출력             ParticipantsMenu               ParticipantsMenuOptionForm
     */
    public string form;
    public object option;
}

[Serializable]
public class PwInputOptionForm
{ 
/*
 *  password : “패스 워드 문자열”
*/
    public string password;
}

[Serializable]
public class ParticipantsMenuOptionForm
{ 
/*
    roomMaker : True/False, //True = 방장
    memberId : “memberId문자열”
*/
    public bool roomMaker;
    public string memberId;
}

[Serializable]
public class ParticipantsListOptionForm
{ 
/*
   list : 아래의 인원 목록 //참석자에 대한 인원목록
   roomId : “roomId 문자열”
    maxClient : N //최대 수용 인원 정수
*/
    public List<FriendInfo> list;
    public string roomId;
    public string tableId;
    public int maxClient;
    public int chatRoomId;
}

[Serializable]
public class InvitableListOptionForm
{ 
/*
   list : 아래의 인원 목록 //참석자에 대한 인원목록
   roomId : “roomId 문자열”
    maxClient : N //최대 수용 인원 정수
*/
    public List<FriendInfo> list;
}

[Serializable]
public class SwitchSceneForm
{ 
/*
url : “link”, // (세미나 || 로비) 영상 링크
roomType : “룸타입 문자열”, //세미나용 룸 크기 “22” - 소형, “21” - 중형, “20” - 대형
roomId: “룸아이디 문자열”,
roomTitle : “룸 타이틀”,//세미나용
videoStartTime : “2023-07-03 20:41:40”, //해당 룸의 영상의 시작 시간
isAppRelease : true/false, //거짓일때 테스트 버전인 bool형
scene : “씬 문자열” 
tableId : “채팅라운지 바로 입장시 테이블아이디 값 적용”
// 씬(“seminar || lobby || KKU || CAU || SMOONU || HONAU”)
*/
    
    public string url;
    public string roomType;
    public string roomId;
    public string roomTitle;
    public string videoStartTime;
    public bool isAppRelease;
    public string scene;
    public string tableId;
}

[Serializable]
public class SwitchSceneSimpleForm
{ 
/*
scene : “씬 문자열” 
// 씬(“SelectView || AvatarView”)

*/
    public string scene;
    public bool isAppRelease;
}

[Serializable]
public class AvatarInfo
{ 
/*
userName : “유저의 이름”
memberId : N //유저의 멤버 아이디 정수형
avatarState : 	{
		    	skinColorCode = "#000000",
                hairCode = "2001",
                hairColorCode = "#000000",
                faceCode = "3001",
                topCode = "4001",
                bottomCode = "5001",
                shoeCode = "6001",
                bodyCode = “default”
                //중성 아바타 form, 새 아바타 적용 전까지 아바타 값 담는용
			}
*/
    
    public string userName;
    public int memberId;
    public AvatarState avatarState;
}

[Serializable]
public class MemberInfoFromNative
{ 
/*
    memberId: Int,
	imgUrl: String,
	birth: String,
	emailId: String,
	name: String,
	phoneNum: String,
	profileName: String,
	status: String,
	universityCode: String,
    locationExposeYn: Int,
	certUniYn: Int

*/
    public int memberId;
    public string imgUrl;
    public string birth;
    public string emailId;
    public string name;
    public string phoneNum;
    public string profileName;
    public string status;
    public string universityCode;
    public int locationExposeYn;
    public int certUniYn;
}

#endregion

