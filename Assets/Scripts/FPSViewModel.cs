using UnityEngine;

public class FPSViewModel : MonoBehaviour
{
    public GameObject characterInstance;
    public RuntimeAnimatorController characterController;
    public Avatar characterAvatar;
    public string idleState = "Idle";
    public string fireState = "Fire";
    public string reloadState = "Reload_Empty";
    public string reloadTacState = "Reload_Tac";

    Animator characterAnimator;
    Animator weaponAnimator;
    float characterAnimatorSpeed = 1f;
    float weaponAnimatorSpeed = 1f;

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
        BindReloadRelay();
        PlayState(characterAnimator, idleState, true);

        weaponAnimator = GetComponentInChildren<Animator>(true);
        if (weaponAnimator != null)
        {
            weaponAnimator.applyRootMotion = false;
            weaponAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            PlayState(weaponAnimator, idleState, true);
        }

        CacheDefaultSpeeds();
    }

    public void PlayFire(bool restartCharacter)
    {
        PlayState(characterAnimator, fireState, restartCharacter);
        PlayState(weaponAnimator, fireState, true);
    }

    public void PlayReload(float duration)
    {
        PlayReload(false, duration);
    }

    public float PlayReload(bool tactical, float emptyReloadTime)
    {
        string stateName = tactical ? ResolveTacState() : reloadState;
        BindReloadRelay();
        PlayState(characterAnimator, stateName, true);
        PlayState(weaponAnimator, stateName, true);

        float wait = emptyReloadTime;
        if (tactical)
            wait = GetTacticalWait(emptyReloadTime, stateName);

        ApplyReloadSpeed(stateName, wait);
        return Mathf.Max(0.05f, wait);
    }

    public void PlayIdle()
    {
        RestoreAnimatorSpeed();
        PlayState(characterAnimator, idleState, true);
        PlayState(weaponAnimator, idleState, true);
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

    void BindReloadRelay()
    {
        if (characterAnimator == null)
            return;

        FPSAnimEventRelay relay = characterAnimator.GetComponent<FPSAnimEventRelay>();
        if (relay == null)
            relay = characterAnimator.gameObject.AddComponent<FPSAnimEventRelay>();

        relay.gun = GetComponent<Gun>();
    }

    void PlayState(Animator animator, string stateName, bool restart)
    {
        if (animator == null || animator.runtimeAnimatorController == null || string.IsNullOrEmpty(stateName))
            return;

        if (!restart)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(stateName))
                return;
        }

        animator.Play(stateName, -1, 0f);
    }

    void ApplyReloadSpeed(string stateName, float duration)
    {
        if (duration <= 0.01f)
            return;

        SetAnimatorSpeed(characterAnimator, GetPlayedClipLength(characterAnimator, stateName) / duration);
        SetAnimatorSpeed(weaponAnimator, GetPlayedClipLength(weaponAnimator, stateName) / duration);
    }

    float GetTacticalWait(float emptyReloadTime, string tacState)
    {
        float tacLength = GetPlayedClipLength(characterAnimator, tacState);
        if (tacLength <= 0.01f)
            tacLength = GetPlayedClipLength(weaponAnimator, tacState);

        float emptyLength = FindClipLength(characterAnimator, reloadState);
        if (emptyLength <= 0.01f)
            emptyLength = FindClipLength(weaponAnimator, reloadState);

        if (emptyLength > 0.01f && tacLength > 0.01f)
            return emptyReloadTime * (tacLength / emptyLength);

        if (tacLength > 0.01f)
            return tacLength;

        return emptyReloadTime;
    }

    string ResolveTacState()
    {
        return string.IsNullOrEmpty(reloadTacState) ? "Reload_Tac" : reloadTacState;
    }

    void CacheDefaultSpeeds()
    {
        characterAnimatorSpeed = characterAnimator != null ? characterAnimator.speed : 1f;
        weaponAnimatorSpeed = weaponAnimator != null ? weaponAnimator.speed : 1f;
    }

    void RestoreAnimatorSpeed()
    {
        if (characterAnimator != null)
            characterAnimator.speed = characterAnimatorSpeed;
        if (weaponAnimator != null)
            weaponAnimator.speed = weaponAnimatorSpeed;
    }

    static void SetAnimatorSpeed(Animator animator, float speed)
    {
        if (animator == null || speed <= 0.01f)
            return;

        animator.speed = speed;
    }

    static float GetPlayedClipLength(Animator animator, string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 0f;

        animator.Update(0f);

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsName(stateName))
        {
            AnimatorClipInfo[] playing = animator.GetCurrentAnimatorClipInfo(0);
            if (playing != null && playing.Length > 0 && playing[0].clip != null)
                return playing[0].clip.length;
            if (info.length > 0f)
                return info.length;
        }

        return FindClipLength(animator, stateName);
    }

    static float FindClipLength(Animator animator, string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null || string.IsNullOrEmpty(stateName))
            return 0f;

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        if (clips == null)
            return 0f;

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i] != null && clips[i].name.IndexOf(stateName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return clips[i].length;
        }

        return 0f;
    }
}

public class FPSAnimEventRelay : MonoBehaviour
{
    public Gun gun;

    public void ReloadComplete()
    {
        if (gun != null)
            gun.ReloadComplete();
    }
}
