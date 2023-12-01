using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DummyController : MonoBehaviour
{


    string[] ColorCode = {
        "#434343", // 0
        "#A43D3D",
        "#FFAE3D",
        "#0F6C3F",
        "#2B5B7B",
        "#D1A4E9",
        "#653625",
        "#B3CFCF", // 7

        "#FAE7D6", // 8
        "#F7DAC6",
        "#F5D5C2",
        "#F1D1B3",
        "#F0C8B4",
        "#EEC2AC",
        "#E7B49B",
        "#DBA58A",
        "#A17766",
        "#845C4E",
        "#614C39",
        "#443521",
        "#D13319", // 20 빨강색
        "#298A3A", // 21 초록색
        "#4DB199", // 22 청록색
        "#23759E" // 23 파랑색
    };


    string[] MeshCode = {
        "AA", //0
        "AB",
        "AC",
        "AD",
        "AE",
        "AF", 
        "AG", //6
        "AH",
        "AI",
        "AJ" };

    string[] NamePiece1List = {
        "빠른",
        "강한",
        "똑똑한",
        "용감한",
        "창의적인",
        "열정적인",
        "친절한",
        "재미있는",
        "귀여운",
        "성실한",
        "우아한",
        "자유로운",
        "활발한",
        "차분한",
        "지혜로운",
        "정직한",
        "용기있는",
        "자신감있는",
        "화려한",
        "행복한",

        "신비로운",
        "감각적인",
        "화려한",
        "모험적인",
        "열정적인",
        "우아한",
        "창조적인",
        "미스터리한",
        "자유로운",
        "활기찬",
        "미래지향적인",
        "강력한",
        "흥미로운",
        "매력적인",
        "유쾌한" };



    string[] NamePiece2List = {
        "곰",
        "사자",
        "표범",
        "기린",
        "하마",
        "악어",
        "판다",
        "판다",
        "늑대",
        "여우",
        "사슴",
        "호랑이",
        "코끼리",
        "코뿔소",
        "캥거루",
        "원숭이",
        "고릴라",
        "족제비",
        "다람쥐",

        "탐험가",
        "아티스트",
        "여행자",
        "엔지니어",
        "음악가",
        "작가",
        "디자이너",
        "천재",
        "드리머",
        "스포츠맨",
        "연구자",
        "요리사",
        "탐정",
        "비밀요원",
        "용사" };

    private List<CharctorMeshAndMaterialController> dummysCtrlArray = null;
    public string[] settingList = null;

    // 이동 목표 용 변수
    public List<Vector3[]> destPosLists = new List<Vector3[]>();

    public GameObject objDummyAvatar;


    // 세팅용 변수
    //public GameObject objlocList;
    //private Transform[] stands;
    //private Transform[] sits;

    // Start is called before the first frame update
    void Start()
    {
        dummysCtrlArray = new List<CharctorMeshAndMaterialController>();//GetComponentsInChildren<CharctorMeshAndMaterialController>();



        // 텍스트 세팅용 변수
        //sits = objlocList.transform.GetChild(0).GetComponentsInChildren<Transform>();
        //stands = objlocList.transform.GetChild(1).GetComponentsInChildren<Transform>();

        //settingList = new string[sits.Length - 1 + stands.Length-1]; // 상수를 넣어야함 

        // 이동 목표 용 변수
        for (int i = 0; i <= 7; i++)
        {
            Transform[] trs = transform.GetChild(i).GetComponentsInChildren<Transform>();
            
            Vector3[] vecs = new Vector3[trs.Length -1];
            int cnt = -1;
            foreach (Transform tr in trs)
            {
                if (cnt < 0)
                {
                    cnt++;
                    continue;
                }
                vecs[cnt++] = tr.position;
            }

            destPosLists.Add(vecs);
        }
        //Vector3[] vecs1 = { new Vector3(50, 0, 50), new Vector3(0, 0, 50), new Vector3(50, 0, 50), new Vector3(50, 0, 0) };
        //Vector3[] vecs2 = { new Vector3(25, 0, 50), new Vector3(0, 0, 50), new Vector3(25, 0, 25), new Vector3(50, 0, -20) };
        //destPosLists.Add(vecs1);
        //destPosLists.Add(vecs2);

        StartCoroutine(WaitDownload());
    }

    IEnumerator WaitDownload()
    {
        while (GameObject.Find("MeshMatContainer(Clone)") == null)
        {
            yield return new WaitForUpdate();
        }


        //// 텍스트 생성 부 
        //int standnum = 1;
        //int sitnum = 1;
        //for (int num = 0; num < settingList.Length; num++)
        //{
        //    string faceMat = Random.Range(1, 14).ToString("D3");
        //    string hairMesh = MeshCode[Random.Range(0, 7)];
        //    string bottomMesh = MeshCode[Random.Range(0, 4)];
        //    string bottomMat = bottomMesh == "AC" ? Random.Range(1, 7).ToString("D3") : Random.Range(1, 4).ToString("D3");
        //    string shoesMesh = MeshCode[Random.Range(0, 3)];
        //    string shoesMat = Random.Range(1, 5).ToString("D3");
        //    string topMesh = MeshCode[Random.Range(0, 5)];
        //    string topMat = (topMesh == "AA" || topMesh == "AB") ? Random.Range(1, 5).ToString("D3") : Random.Range(1, 8).ToString("D3");
        //    string hairColor = ColorCode[Random.Range(0, 8)];
        //    string skinColor = ColorCode[Random.Range(8, 20)];

        //    string name = NamePiece1List[Random.Range(0, NamePiece1List.Length)] + NamePiece2List[Random.Range(0, NamePiece2List.Length)];
        //    string pos = "";
        //    if (standnum < 22)
        //    {
        //        pos = "stand_" + stands[standnum].position.x + "/" + stands[standnum].position.y + "/" + stands[standnum].position.z + "/" + stands[standnum].rotation.eulerAngles.x + "/" + stands[standnum].rotation.eulerAngles.y + "/" + stands[standnum].rotation.eulerAngles.z;
        //        standnum++;
        //    }
        //    else
        //    {
        //        pos = "sit_" + sits[sitnum].position.x + "/" + sits[sitnum].position.y + "/" + sits[sitnum].position.z + "/" + sits[sitnum].rotation.eulerAngles.x + "/" + sits[sitnum].rotation.eulerAngles.y + "/" + sits[sitnum].rotation.eulerAngles.z;
        //        sitnum++;
        //    }

        //    string text = "TOP_" + topMesh + "_" + topMat + "#000000," +
        //        "SHOES_" + shoesMesh + "_" + shoesMat + "#000000," +
        //        "BOTTOM_" + bottomMesh + "_" + bottomMat + "#000000," +
        //        "FACE_AA_" + faceMat + "#000000," +
        //        "HAIR_" + hairMesh + "_001" + hairColor + "," +
        //        "SKIN_AA_001" + skinColor + "," +
        //        name + "," +
        //        pos;
        //    settingList[num] = text;
        //}

        // 텍스트로 더미 옷차림 생성하는 부분
        //TOP_AC_005#000000,SHOES_AB_001#000000,BOTTOM_AC_003#000000,FACE_AA_005#000000,HAIR_AG_001#434343,SKIN_AA_001#A17766,차분한표범,sit_-61.66327/14.25454/-73.90968/270/225/0
        int rootNum = 0;
        int destNum = 0;
        int standNum = 0;
        for (int i = 0; i < settingList.Length; i++)
        {
            GameObject dummyObj = GameObject.Instantiate(objDummyAvatar);
            dummyObj.transform.SetParent(transform);
            dummysCtrlArray.Add(dummyObj.GetComponent<CharctorMeshAndMaterialController>());
            DummyActionController dummy = dummysCtrlArray[i].GetComponent<DummyActionController>();
            string str = settingList[i];
            string[] temps = str.Split(',');
            for (int j = 0; j < temps.Length - 2; j++)
            {
                string set = temps[j];
                string[] matcolor = set.Split('#');
                dummysCtrlArray[i].CharactorPartsChange(matcolor[0], "#" + matcolor[1]);
            }
            TextMeshPro tmp = dummysCtrlArray[i].GetComponentInChildren<TextMeshPro>();
            tmp.text = temps[temps.Length - 2];
            string[] loc = temps[temps.Length - 1].Split('_');
            bool isSit = loc[0].Equals("sit");
            if (isSit)
            {
                rootNum = 0;
                destNum = 0;
            }
            else
            {
                if (standNum++ < 10)
                {
                    if (standNum++ < 5) { 
                        dummy.isSprint = true;
                    }
                    rootNum++;
                    if (rootNum >= 8) { rootNum = 1; destNum++; if (destNum >= 10) destNum = 0; }                    
                    
                }
                else
                {
                    rootNum = 0;
                    destNum = 0;
                }
            }
            //Debug.Log("i " + i + " rootNum " + rootNum + " destNum " + destNum);
            loc = loc[1].Split('/');
            dummy.StartPos = new Vector3(float.Parse(loc[0]), float.Parse(loc[1]), float.Parse(loc[2]));
            dummy.StartRot = Quaternion.Euler(new Vector3(0, float.Parse(loc[4]) - 180, 0));//Quaternion.Euler(new Vector3(0, float.Parse(loc[3]), 0));
            dummy.Init(rootNum, destNum, destPosLists[rootNum]);
            dummy.isSit = isSit;
            
            //dummy.gameObject.SetActive(true);
            //dummysCtrlArray[i].GetComponent<DummyActionController>().dests = destPosLists[Random.Range(0, destPosLists.Count)]; // 이동 좌표 리스트 할당
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
