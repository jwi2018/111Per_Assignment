using UnityEngine;

public abstract class EnemySkillBase : MonoBehaviour
{
    [field: SerializeField] public int SkillID { get; protected set; }

    protected Enemy _enemy;
    protected Transform _firePoint;
    protected GameObject _enemyArrowPrefab; 
    protected Rigidbody2D _enemyRigidbody;
    protected EnemyController _enemyController;

    public virtual void Initialize(Enemy enemy, Transform firePoint, GameObject enemyArrowPrefab, Rigidbody2D enemyRigidbody, EnemyController enemyController)
    {
        _enemy = enemy;
        _firePoint = firePoint;
        _enemyArrowPrefab = enemyArrowPrefab;
        _enemyRigidbody = enemyRigidbody;
        _enemyController = enemyController;
    }

    public abstract bool CanActivate();

    public abstract void Activate();

    public virtual void OnSkillEnd()
    {
        _enemy.SetEnemyState(EnemyState.Idle); 
        _enemy.TryUseSkill(SkillID);
        _enemyController?.RestoreEnemyStateAfterSkillEnd();
    }

    protected void PlayAttackSound()
    {
        _enemy.PlayAttackSound();
    }
}