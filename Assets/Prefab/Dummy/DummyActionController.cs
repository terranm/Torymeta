using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DummyActionController : MonoBehaviour
{
    bool isStart = false;
    private int rootNumber = 0;

    //// Start is called before the first frame update
    public void Init(int root, int dest, Vector3[] inputdests)
    {
        rootNumber = root;
        destNum = dest;
        if (rootNumber > 0)
        {
            dests = inputdests;
            destPos = dests[destNum++];
            transform.position = destPos;
            if (dests.Length == destNum) destNum = 0;
        }
        else
        {
            transform.position = StartPos;
            transform.rotation = StartRot;
        }
        anim = transform.GetComponent<AnimationController>().anim;
        StartCoroutine(WaitAnimCtrl());
    }

    IEnumerator WaitAnimCtrl()
    {
        AnimationController animCtrl;
        while (!transform.TryGetComponent<AnimationController>(out animCtrl))
        {
            yield return new WaitForUpdate();
        }
        while (animCtrl.anim == null)
        {
            yield return new WaitForUpdate();
        }
        anim = animCtrl.anim;
        isStart = true;
    }

    public Vector3[] dests;
    public Vector3 StartPos;
    public Quaternion StartRot;
    private int destNum = 0;

    Animator anim;

    //// Update is called once per frame
    void Update()
    {
        if (!isStart) return;
        if (isSit)
        {
            anim.SetBool("isSit", true);
        }
        else
        {
            if (rootNumber > 0)
            {
                DestCheck();
                CalcurateNextPos();
                Move();
            }
        }
    }

    public bool isSit = false;
    public bool isSprint = false;

    private float maxAngleForSnapRotation = 35f;
    private float positionLerpSpeed = 5f;
    private float rotationLerpSpeed = 5f;
    private float magnitude = 10;
    private Vector3 prevPosition = Vector3.zero;
    private Vector3 destPos;
    private Vector3 nextPos;
    private float speed = 0.0f;
    

    private void DestCheck()
    {
        if ((destPos - transform.position).magnitude < 1)
        {
            transform.position = destPos;
            destPos = dests[destNum++];
            if (dests.Length == destNum) destNum = 0;
        }
    }

    private void CalcurateNextPos()
    {

        float dis = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(destPos.x, destPos.z));

        nextPos = transform.position + (destPos - transform.position).normalized * 3;
    }

    private void Move()
    {
        transform.position = Vector3.Lerp(transform.position, nextPos,
            Time.deltaTime * (isSprint ? positionLerpSpeed : (positionLerpSpeed / 3 * 2)));// * 2.5f);

        if (math.abs(transform.position.y - nextPos.y) > 10)
            transform.position = new Vector3(transform.position.x, nextPos.y, transform.position.z);

        float dis = Vector2.Distance(new Vector2(prevPosition.x, prevPosition.z),
            new Vector2(transform.position.x, transform.position.z));

        speed = Mathf.Lerp(speed, dis > 0.1f ? (isSprint ? 15: 10) : 0, Time.deltaTime * magnitude);

        // animation 동작
        anim.SetFloat("Speed", speed);

        transform.LookAt(nextPos);

        //if (Mathf.Abs(Quaternion.Angle(transform.rotation, destRot)) > maxAngleForSnapRotation)
        //    transform.rotation = destRot;
        //else
        //    transform.rotation = Quaternion.Slerp(transform.rotation, destRot,
        //        Time.deltaTime * (rotationLerpSpeed + 50));

        prevPosition = transform.position;
    }
}
