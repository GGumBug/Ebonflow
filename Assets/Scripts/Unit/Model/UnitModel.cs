using AutoBattle;
using UnityEngine;

public class UnitModel : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private Transform _rootTransform;
    private UnitAnimationController _unitAnimationController;
    private ShootPositionGenerator _shootPositionGenerator;

    private void Awake()
    {
        _unitAnimationController = new UnitAnimationController(animator);
        _shootPositionGenerator = new ShootPositionGenerator();
    }

    public void SetAnimatorController(Transform rootTransform, AddressableKey modelKey)
    {
        _rootTransform = rootTransform;
        var controller = AutoBattleDataManager.Instance.GetUnitModelAnimator(modelKey);
        _unitAnimationController.SetAnimatorOverrideController(controller);
    }

    public Vector2 GetShootPosition()
    {
        int directionIndex = _unitAnimationController.GetCurrentDirectionIndex();

        return _shootPositionGenerator.GetShootPositionFromIndex(_rootTransform.position, directionIndex);
    }

    public void StopWalkAnimation() => _unitAnimationController.StopWalk();
    public void PlayWalkAnimation() => _unitAnimationController.SetWalk();
    public void TriggerUnitAttack() => _unitAnimationController.TriggerAttack();
    public void TriggerUnitSkill() => _unitAnimationController.TriggerSkill();
    public void SetDead(bool isDead) => _unitAnimationController.SetDead(isDead);
    public void SetUnitDirection(Vector2 dir) => _unitAnimationController.SetDirection(dir);
}
