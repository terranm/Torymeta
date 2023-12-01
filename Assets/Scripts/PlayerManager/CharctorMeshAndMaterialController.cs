using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System;

public class CharctorMeshAndMaterialController : MonoBehaviour
{
	//[SerializeField]
	//SkinnedMeshRenderer targetSkinnedMeshRenderer;
	//[SerializeField]
	//SkinnedMeshRenderer newSkinnedMeshRenderer;
	//[SerializeField]
	//Material newMaterial;

	public SkinnedMeshRenderer[] originalSkinnedMeshRenderers;
	public SkinnedMeshRenderer[] skinnedMeshRendererList;
	public Material[] materialList;

    private void Awake()
    {
		if (GameObject.Find("MeshMatContainer(Clone)") == null)
		{
			DownloadManager.Instance.CharacterContainerDownload();
		}
		StartCoroutine(Init());		
	}

    public IEnumerator Init()
	{
		GameObject container = GameObject.Find("MeshMatContainer(Clone)");
		while (container == null)
		{
			yield return new WaitForUpdate();
			container = GameObject.Find("MeshMatContainer(Clone)");
		}
		//Resources.LoadAll<Material>("material"); // 메테리얼 프리팹으로 만들어서 가져오는 법 새로 개발해야함 
		originalSkinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
		skinnedMeshRendererList = container.GetComponentsInChildren<SkinnedMeshRenderer>();
		materialList = container.GetComponent<MaterialContainer>().materialList;
		Debug.Log("originalMeshRenderers : " + originalSkinnedMeshRenderers.Length +"\nskinnedMeshRendererList : " + skinnedMeshRendererList.Length
		+ "\nmaterialList : " + materialList.Length);
		// 테스트용 캐릭 초기화
		//CharacterSetting();
		yield return null;
	}

	#region UNITYC_CALLBACK


#if UNITY_EDITOR
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1)) // bottom
		{
			CharactorPartsChange("BOTTOM", "AB", "001");
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			CharactorPartsChange("BOTTOM", "AA", "002");
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3)) // skin
		{
			CharactorPartsChange("SKIN", "AA", "", "#D13319");
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			CharactorPartsChange("SKIN", "AA", "", "#FAE7D6");
		}
		else if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			CharactorPartsChange("SKIN", "AA", "", "#D13319");
			CharactorPartsChange("TOP", "AA", "001");
			CharactorPartsChange("SHOES", "AA", "001");
			CharactorPartsChange("BOTTOM", "AA", "001");
			CharactorPartsChange("FACE", "AA", "002");
			CharactorPartsChange("HAIR", "AA", "001", "#434343");
		}
		else if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			CharactorPartsChange("SKIN", "AA", "", "#FAE7D6");
			CharactorPartsChange("TOP", "AB", "001");
			CharactorPartsChange("SHOES", "AB", "001");
			CharactorPartsChange("BOTTOM", "AB", "001");
			CharactorPartsChange("FACE", "AA", "001");
			CharactorPartsChange("HAIR", "AB", "", "#434343");
		}
		else if (Input.GetKeyDown(KeyCode.Alpha7))
		{

			CharactorPartsChange("HAIR", "AC", "001", "#434343");
		}
		else if (Input.GetKeyDown(KeyCode.Alpha8))
		{

			CharactorPartsChange("HAIR", "AD", "001", "#434343");
		}
	}

	void OnDrawGizmosSelected()
	{
		var meshrenderer = GetComponentInChildren<SkinnedMeshRenderer>();
		Vector3 before = meshrenderer.bones[0].position;
		for (int i = 0; i < meshrenderer.bones.Length; i++)
		{
			Gizmos.DrawLine(meshrenderer.bones[i].position, before);
			UnityEditor.Handles.Label(meshrenderer.bones[i].transform.position, i.ToString());
			before = meshrenderer.bones[i].position;
		}
	}
#endif

	#endregion

	public void CharactorPartsChange(string code, string color)
    {
		string[] splitCodes = code.Split('_');
		CharactorPartsChange(splitCodes[0], splitCodes[1], splitCodes[2], color.ToUpper());

	}

	public void CharacterSetting()
	{
		StartCoroutine(WaitUntilContainerAndThenSetting());
	}

	private IEnumerator WaitUntilContainerAndThenSetting()
	{
		GameObject container = GameObject.Find("MeshMatContainer(Clone)");
		while (container == null)
		{
			yield return new WaitForUpdate();
			container = GameObject.Find("MeshMatContainer(Clone)");
		}

		while (!(PlayerData.myPlayerinfo.state != null && skinnedMeshRendererList.Length > 0))
		{
			yield return new WaitForUpdate();
		}
		
		CharactorPartsChange(PlayerData.myPlayerinfo.state.faceCode, PlayerData.myPlayerinfo.state.faceColorCode);
		CharactorPartsChange(PlayerData.myPlayerinfo.state.hairCode, PlayerData.myPlayerinfo.state.hairColorCode);
		CharactorPartsChange(PlayerData.myPlayerinfo.state.skinCode, PlayerData.myPlayerinfo.state.skinColorCode);
		CharactorPartsChange(PlayerData.myPlayerinfo.state.topCode, PlayerData.myPlayerinfo.state.topColorCode);
		CharactorPartsChange(PlayerData.myPlayerinfo.state.bottomCode, PlayerData.myPlayerinfo.state.bottomColorCode);
		CharactorPartsChange(PlayerData.myPlayerinfo.state.shoesCode, PlayerData.myPlayerinfo.state.shoesColorCode);
		yield return null;
	}

	public void CharacterSetting(AvatarState state)
	{
		if (state != null)
		{
			CharactorPartsChange(state.faceCode, state.faceColorCode);
			CharactorPartsChange(state.hairCode, state.hairColorCode);
			CharactorPartsChange(state.skinCode, state.skinColorCode);
			CharactorPartsChange(state.topCode, state.topColorCode);
			CharactorPartsChange(state.bottomCode, state.bottomColorCode);
			CharactorPartsChange(state.shoesCode, state.shoesColorCode);
		}
        else
        {
			Debug.LogError("Charactor state Null");
        }
	}

	private void CharactorPartsChange(string partsName, string meshName, string matName, string matColor = "#000000")
    {
		//Debug.Log("CharactorPartsChange " + partsName + "_" + meshName + "_" + matName + " " + matColor);
        switch (partsName)
		{
			case "SKIN":
				UpdateMaterial("BOTTOM", partsName + "_" + meshName + "_" + matColor, 1);
				UpdateMaterial("TOP", partsName + "_" + meshName + "_" + matColor, 1);
				UpdateMaterial("FACE", partsName + "_FACE_" + meshName + "_" + matColor, 1);
				break;

			case "HAIR":
				UpdateMeshRenderer(partsName, partsName + "_" + meshName);
				//if (meshName == "AA") // AA의 경우 모자와 머리카락 메쉬 위치가 바뀌어 있음
				//{
				//	UpdateMaterial(partsName, partsName + "_" + meshName + "_" + matColor, 1);
				//	UpdateMaterial(partsName, partsName + "_" + meshName + "_" + matName);
				//}
                //else
                //{
					UpdateMaterial(partsName, partsName + "_" + meshName + "_" + matColor);
					UpdateMaterial(partsName, partsName + "_" + meshName + "_" + matName, 1);
				//}
				break;

			case "FACE":
				UpdateMeshRenderer(partsName, partsName + "_" + meshName);
				UpdateMaterial(partsName, partsName + "_" + meshName + "_" + "001"); // 기본 얼굴
				UpdateMaterial(partsName, "SKIN_" + partsName + "_" + meshName + "_#" + ColorUtility.ToHtmlStringRGB(Array.Find<SkinnedMeshRenderer>(originalSkinnedMeshRenderers, c => c.name == partsName).materials[1].color), 1);// 피부색 수정
				UpdateMaterial(partsName, partsName + "_" + meshName + "_" + matName, 2); // 문양 추가
				if(matName == "001") // 기본얼굴의 경우 문양 삭제
				{
					UpdateMaterial(partsName, "delete", 2);
				}
				CharactorPartsChange("EYE", meshName, "001");
				break;

			case "EYE":
				UpdateMeshRenderer(partsName, partsName + "_" + meshName);
				UpdateMaterial(partsName, "FACE" + "_" + meshName + "_" + matName);
				break;
			case "BOTTOM":
			case "TOP":
			case "SHOES":
				UpdateMeshRenderer(partsName, partsName + "_" + meshName);
				UpdateMaterial(partsName, partsName + "_" + meshName + "_" + matName);
				break;
			default:
				Debug.Log("Unidentified parts name - " + partsName);
				break;
		}
	}

	public void UpdateMeshRenderer(string targetName, string newMeshName)
	{
		//Debug.Log(targetName + " " + newMeshName);
		SkinnedMeshRenderer targetMeshRenderer = Array.Find<SkinnedMeshRenderer>(originalSkinnedMeshRenderers, c => c.name == targetName);
		SkinnedMeshRenderer newMeshRenderer = Array.Find<SkinnedMeshRenderer>(skinnedMeshRendererList, c => c.name == newMeshName);
		// update mesh
		targetMeshRenderer.sharedMesh = newMeshRenderer.sharedMesh;

		Transform[] childrens = transform.GetComponentsInChildren<Transform>(true);

		// sort bones.
		Transform[] bones = new Transform[newMeshRenderer.bones.Length];
		for (int boneOrder = 0; boneOrder < newMeshRenderer.bones.Length; boneOrder++)
		{
			bones[boneOrder] = Array.Find<Transform>(childrens, c => c.name == newMeshRenderer.bones[boneOrder].name);
		}
		targetMeshRenderer.bones = bones;
	}

	//private void ResizeMaterials(SkinnedMeshRenderer targetMeshRenderer, Material newMaterial, int materialNum)
 //   {
	//	if (newMaterial == null) // 입력된 마테리얼 값이 없는 경우
	//	{
	//		Material[] temp = targetMeshRenderer.materials;
	//		//if (temp.Length <= 1) // 메테리얼이 1개만 있는 경우 예외처ㄹ
	//		//	return false;
	//		Array.Resize(ref temp, temp.Length - 1);
	//		targetMeshRenderer.materials = temp;
	//		return;
	//	}

	//	if (targetMeshRenderer.materials.Length <= materialNum) // 현재 마테리얼의 개수가 입력예정인 순서값보다 낮아서 할당이 불가능할때
	//	{
	//		Material[] temp = targetMeshRenderer.materials;
	//		Array.Resize(ref temp, temp.Length + 1);
	//		temp.SetValue(null, temp.Length - 1);
	//		targetMeshRenderer.materials = temp;
	//	}

	//	//return false;
	//}

	private void UpdateMaterial(string targetName, string newMeshName, int materialNum = 0)
	{
		//Debug.Log(targetName + " " + newMeshName + " " + materialNum);
		SkinnedMeshRenderer targetMeshRenderer = Array.Find<SkinnedMeshRenderer>(originalSkinnedMeshRenderers, c => c.name == targetName);
		Material newMaterial = Array.Find<Material>(materialList, c => c.name == newMeshName);

		if (targetMeshRenderer.materials.Length <= materialNum) // 현재 메쉬에 마테리얼의 개수가 입력예정인 순서값보다 낮아서 할당이 불가능할때
		{
			Material[] temp = targetMeshRenderer.materials;
			Array.Resize(ref temp, temp.Length + 1);
			temp.SetValue(null, temp.Length - 1);
			targetMeshRenderer.materials = temp;
		}

		if (newMaterial == null) // 입력된 마테리얼 값이 없는 경우
		{
			Material[] temp = targetMeshRenderer.materials;
			//if (temp.Length <= 1) // 메테리얼이 1개만 있는 경우 예외처ㄹ
			//	return false;
			Array.Resize(ref temp, temp.Length - 1);
			targetMeshRenderer.materials = temp;
			return;
		}

		Material[] tempMat = targetMeshRenderer.materials;
		tempMat[materialNum] = newMaterial;
		targetMeshRenderer.materials = tempMat;
		//ResizeMaterials(targetMeshRenderer, newMaterial, materialNum);
	}
}