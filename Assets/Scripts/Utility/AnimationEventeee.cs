using UnityEngine;

public class AnimationEventeee : MonoBehaviour
{
    private Animator anim;
    
    public void QuitAnimation(string animName)
    {
        if (anim == null)
            anim = GetComponent<Animator>();
        
        anim.SetBool(animName, false);
    }
}
