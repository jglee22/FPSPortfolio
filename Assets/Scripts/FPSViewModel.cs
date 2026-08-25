using UnityEngine;

public class FPSViewModel : MonoBehaviour
{
    public GameObject characterInstance;
    public RuntimeAnimatorController characterController;
    public Avatar characterAvatar;
    public string idleState = "Idle";
    public string fireState = "Fire";
    public string reloadState = "Reload_Empty";

    Animator characterAnimator;
    Animator weaponAnimator;

    public bool IsReady => characterAnimator != null;

    public void Build()
    {
        if (characterInstance == null)
        {
            Debug.LogError("FPSViewModel: Character Instance에 씬의 SK_Arms_Mono를 넣으세요.", this);
            return;
        }

        characterAnimator = SetupAnimator(characterInstance, characterController, characterAvatar);
        PrepareViewRenderers(characterInstance);
        PlayState(characterAnimator, idleState);

        weaponAnimator = GetComponentInChildren<Animator>(true);
        if (weaponAnimator != null)
        {
            weaponAnimator.applyRootMotion = false;
            weaponAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            PlayState(weaponAnimator, idleState);
        }
    }

    public void PlayFire()
    {
        PlayState(characterAnimator, fireState);
        PlayState(weaponAnimator, fireState);
    }

    public void PlayReload()
    {
        PlayState(characterAnimator, reloadState);
        PlayState(weaponAnimator, reloadState);
    }

    Animator SetupAnimator(GameObject root, RuntimeAnimatorController controller, Avatar avatar)
    {
        Animator animator = root.GetComponentInChildren<Animator>(true);
        if (animator == null)
            animator = root.AddComponent<Animator>();

        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        if (avatar != null)
            animator.avatar = avatar;
        if (controller != null)
            animator.runtimeAnimatorController = controller;

        return animator;
    }

    void PrepareViewRenderers(GameObject root)
    {
        SkinnedMeshRenderer[] skinned = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < skinned.Length; i++)
        {
            skinned[i].enabled = true;
            skinned[i].forceRenderingOff = false;
            skinned[i].updateWhenOffscreen = true;
        }
    }

    void PlayState(Animator animator, string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null || string.IsNullOrEmpty(stateName))
            return;

        animator.Play(stateName, -1, 0f);
    }
}
