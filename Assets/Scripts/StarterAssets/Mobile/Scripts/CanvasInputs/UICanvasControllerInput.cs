using UnityEngine;
using UnityEngine.TextCore.Text;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {
        private NetworkedEntity entity;
        private Animator anim;
        private CharacterController controller;
        
        [Header("Output")]
        public StarterAssetsInputs starterAssetsInputs;

        
        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            starterAssetsInputs?.MoveInput(virtualMoveDirection);

            if (entity == null) entity = NetworkedEntityFactory.Instance.GetMine();
            if (anim == null) anim = entity.GetComponent<Animator>();
            if (controller == null) controller = entity.GetComponent<CharacterController>();
            
            controller.enabled = true;
            
            SetAnimation();
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            starterAssetsInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            starterAssetsInputs?.JumpInput(virtualJumpState);

            if (entity == null) entity = NetworkedEntityFactory.Instance.GetMine();

            entity.SendRFC("isJumpStart");
        }

        public void VirtualSprintInput()
        {
            bool newSprint = !starterAssetsInputs.sprint;
            starterAssetsInputs?.SprintInput(newSprint);
            this.transform.Find("UI_Virtual_Button_Run").GetChild(newSprint ? 0 : 1).gameObject.SetActive(false);
            this.transform.Find("UI_Virtual_Button_Run").GetChild(newSprint ? 1 : 0).gameObject.SetActive(true);
        }


        private void SetAnimation()
        {
            if(anim.GetBool("isSit") || anim.GetBool("isItsMe"))
                entity.SendRFC("Stand");
            
            // anim.SetBool("OnEmotion", false);
            anim.SetBool("isSit", false);
            //anim.SetBool("HelloTrigger",false);
            //anim.SetBool("FightingTrigger", false);
            anim.SetBool("isItsMe",false);
        }
    }

}
