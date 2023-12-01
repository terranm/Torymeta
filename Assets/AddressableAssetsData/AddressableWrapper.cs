using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Torymeta.Addressable
{
    public class AddressableWrapper
    {
        public static bool isTest = true;

        public static string path = "";
        
        public static string remotePath
        {
            get
            {
                if (isTest)
                    path = "https://test-metabus-unity.s3.ap-northeast-2.amazonaws.com/2.2.005/";
                else
                    path = "https://metabus-unity.s3.ap-northeast-2.amazonaws.com/2.2.000/";

                Debug.Log("[Debug] runtime Path : " + path);
                return path;
            }
            set { path = value; }
        }

        // https://test-metabus-unity.s3.ap-northeast-2.amazonaws.com/address/ios
        // {Torymeta.Addressable.AddressableWrapper.remotePath}/[BuildTarget]
        // https://test-metabus-unity.s3.ap-northeast-2.amazonaws.com/address/aos
    }
}
