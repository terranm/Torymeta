using System;
using Colyseus;
using Colyseus.Schema;
using GameDevWare.Serialization;
using LucidSightTools;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Utility;

public class CreateUserMenu : MonoBehaviour
{
    // Primary view components
    //====================================
    [SerializeField]
    private LobbyController lobbyController;

    /// <summary>
    /// Make a request to the serve to log a user in.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="roomId"></param>
    /// <param name="onError">Callback to execute in the event of an error occurs with the log in attempt.</param>
    public void TryConnect(string roomId, Action<string> onError)
    {
        try
        {
            MMOManager.Instance.UserLogIn<UserAuthResponse>(roomId, (response) => { JoinById(onError, response); });
        }
        catch (Exception e)
        {
            Debug.Log("TryConnect() - exception : "+ e.StackTrace);
        }
    }
    
    private async void JoinById(Action<string> onError, RequestResponse response)
    {
        if (response.error)
        {
            onError?.Invoke(response.output);
        }
        else
        {
            try
            {
                UserAuthResponse userAuthResponse = (UserAuthResponse)response;

                Response root = JsonConvert.DeserializeObject<Response>(response.rawResponse);
                ColyseusRoom<RoomState> room = await MMOManager.Instance.JoinById(root.roomId);

                MMOManager.Instance.Room = room;
                MMOManager.Instance.RegisterHandlers();
                
                if (userAuthResponse.output.seatReservation.room == null)
                {
                    userAuthResponse.output.seatReservation = new SeatReservationData();
                    userAuthResponse.output.seatReservation.room = new ColyseusRoomAvailable()
                    {
                        roomId = room.Id,
                        name = "toryworld"
                    };
                }
                else
                {
                    userAuthResponse.output.seatReservation.room = root.output.seatReservation;
                    userAuthResponse.output.seatReservation.room.roomId = root.roomId;
                    userAuthResponse.output.seatReservation.sessionId = room.SessionId;
                }

                Debug.Log($"Session Id = {room.SessionId}");

                PlayerData.myPlayerinfo.entityId = room.SessionId;
                
                userAuthResponse.output.user = new UserData()
                {
                    id = room.SessionId,
                    username = PlayerData.myPlayerinfo.userName
                };
                
                lobbyController.ConsumeSeatReservation(userAuthResponse);
            }
            catch (Exception e)
            {
                Debug.Log("JoinById() - exception : "+ e.StackTrace);
            }
        }
    }
}


public class Response
{
    public string roomId;
    public output output;
}

public class output
{
    public seatReservation seatReservation;
}

[Serializable]
public class seatReservation : ColyseusRoomAvailable
{
}