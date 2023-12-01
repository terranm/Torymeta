using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animator _animator = null;

    public Animator anim { get { if(_animator != null) return _animator; return null; } }

    private StarterAssets.ThirdPersonController _tPCtrl = null;
    //[SerializeField] private AnimationController animCtrl = null;
    private bool Grounded = false;

    private float GroundedOffset = -0.14f;
    private float GroundedRadius = 0.7f;
    private LayerMask GroundLayers;

    private float FallTimeout = 0.15f;

    private float JumpHeight = 1.2f;
    private float Gravity = -15.0f;
    public float JumpTimeout = 0.50f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;


    private int _animIDSpeed;
    //private int _animIDMotionSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDisSit;

    private bool _hasAnimator;

    public bool isJump;

    // Start is called before the first frame update
    void Start()
    {
        if (transform.TryGetComponent<StarterAssets.ThirdPersonController>(out _tPCtrl))
        {
            Grounded = _tPCtrl.Grounded;
            GroundedOffset = _tPCtrl.GroundedOffset;
            GroundedRadius = _tPCtrl.GroundedRadius;
            GroundLayers = _tPCtrl.GroundLayers;
            FallTimeout = _tPCtrl.FallTimeout;
            JumpHeight = _tPCtrl.JumpHeight;
            Gravity = _tPCtrl.Gravity;
            JumpTimeout = _tPCtrl.JumpTimeout;

            _fallTimeoutDelta = FallTimeout;

            _tPCtrl.enabled = false;
        }
        else
        {
            gameObject.SetActive(false);
        }
        _hasAnimator = transform.TryGetComponent<Animator>(out _animator);

        if (_hasAnimator)
        {
            AssignAnimationIDs();
        }
        else
        {
            Debug.Log("Other Animation error");
        }
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("isGrounded");
        _animIDJump = Animator.StringToHash("isJumpStart");
        _animIDFreeFall = Animator.StringToHash("isFreeFall");
        //_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDisSit = Animator.StringToHash("isSit");
    }

    // Update is called once per frame
    void Update()
    {
        // 테스트용 키보드 입력 버튼
        //if (Input.GetKeyDown(KeyCode.Z))
        //    RunAnimationFloat("Walk&Run", 0);
        //if (Input.GetKeyDown(KeyCode.X))
        //    RunAnimationFloat("Walk&Run", 50);
        //if (Input.GetKeyDown(KeyCode.C))
        //    RunAnimationFloat("Walk&Run", 100);
        //if (Input.GetKeyDown(KeyCode.V))
        //    RunAnimationBool("isSit");
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    RunAnimationJump();
        //}
        _hasAnimator = TryGetComponent(out _animator);

        JumpAndGravity();
        GroundedCheck();
    }

    private void GroundedCheck()
    {
        if (_animator.GetBool(_animIDisSit))
        {
            _animator.SetBool(_animIDGrounded, true);
            Grounded = true;
            return;
        }
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (isJump && _jumpTimeoutDelta <= 0.0f)
            {
                isJump = false;
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }

            // if we are not grounded, do not jump
            isJump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }


    //public void RunAnimationJump()
    //{
    //    SetAllStopAnimation();
    //    RunAnimationInteger("JumpState", 1);
    //    RunAnimationBool("isInAir");
    //    StartCoroutine(Jump());
    //}

    //IEnumerator Jump()
    //{
    //    yield return new WaitForSeconds(0.533f);
    //    RunAnimationInteger("JumpState", 2);
    //    yield return new WaitForSeconds(0.5f); // 실체공 시간
    //    RunAnimationBool("isInAir");
    //    yield return new WaitForSeconds(0.667f); // 실체공 시간
    //    RunAnimationInteger("JumpState", 0);
    //}

    //public void SetAllStopAnimation()
    //{
    //    animator.SetFloat("Walk&Run", 0);
    //    animator.SetBool("isSit", false);
    //    animator.SetInteger("JumpState", 0);
    //    animator.SetBool("isInAir", false);
    //    animator.SetTrigger("ResetTrigger");
    //}

    ///// <summary>
    ///// 애니메이션 Integer 실행 함수
    ///// </summary>
    /////<param name="name">실행할 파라메터명</param>
    /////<param name="param">실행할 파라메터</param>
    //public void RunAnimationInteger(string name, int param)
    //{
    //    animator.SetInteger(name, param);
    //}

    ///// <summary>
    ///// 애니메이션 float 실행 함수
    ///// </summary>
    /////<param name="name">실행할 파라메터명</param>
    /////<param name="param">실행할 파라메터</param>
    //public void RunAnimationFloat(string name, float param)
    //{
    //    animator.SetFloat(name, param);
    //}

    ///// <summary>
    ///// 애니메이션 Bool 현재상태 반RunAnimationBool 함수
    ///// </summary>
    /////<param name="name">실행할 Bool 파라메터명</param>
    //public void RunAnimationBool(string name)
    //{
    //    animator.SetBool(name, !animator.GetBool(name));
    //}

    ///// <summary>
    ///// 애니메이션 트리거 실행 함수
    ///// JumpTrigger, HelloTrigger, FightingTrigger, ItsmeTrigger
    ///// </summary>
    /////<param name="Trigger">실행할 트리거명</param>
    //public void RunAnimationTrigger(string name)
    //{
    //    animator.SetTrigger(name);
    //}

}
