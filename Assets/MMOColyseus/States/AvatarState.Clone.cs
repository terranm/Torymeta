using Colyseus.Schema;
using UnityEngine.Networking;

public partial class AvatarState : Schema {
    public AvatarState Clone()
    {
        return new AvatarState()
        {
            skinCode = skinCode,
            skinColorCode = skinColorCode,
            hairCode = hairCode,
            hairColorCode = hairColorCode,
            faceCode = faceCode,
            faceColorCode = faceColorCode,
            topCode = topCode,
            topColorCode = topColorCode,
            bottomCode = bottomCode,
            bottomColorCode = bottomColorCode,
            shoesCode = shoesCode,
            shoesColorCode = shoesColorCode,
            bodyCode = bodyCode
        };
    }

    public object[] ToNetSendObjects()
    {
        return new object[] { 
            skinCode,
            skinColorCode ,
            hairCode,
            hairColorCode,
            faceCode,
            faceColorCode,
            topCode,
            topColorCode,
            bottomCode,
            bottomColorCode,
            shoesCode,
            shoesColorCode,
            bodyCode
        };
    }
}

