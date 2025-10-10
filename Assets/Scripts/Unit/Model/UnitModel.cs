using UnityEngine;

public class UnitModel : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    private SpriteOffsetAdjuster _spriteOffsetAdjuster;
    private UnitAnimationController _unitAnimationController;

    private void Awake()
    {
        _spriteOffsetAdjuster = gameObject.AddComponent<SpriteOffsetAdjuster>();
        _unitAnimationController = new UnitAnimationController(animator);
    }

    public void StopWalkAnimation() => _unitAnimationController.StopWalk();
    public void PlayWalkAnimation() => _unitAnimationController.SetWalk();
    public void TriggerUnitAttack() => _unitAnimationController.TriggerAttack();
    public void TriggerUnitSkill() => _unitAnimationController.TriggerSkill();
    public void SetDead(bool isDead) => _unitAnimationController.SetDead(isDead);
    public void SetUnitDirection(Vector2 dir) => _unitAnimationController.SetDirection(dir);
}
