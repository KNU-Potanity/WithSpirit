using System.Collections;
using BasePlatformer.Monsters;
using UnityEngine;

public class FairyAttackController : MonoBehaviour
{
    [Header("Fairy Data")]
    public MainFairyData fairyData;

    [Header("Attack Motion")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float returnSpeed = 18f;
    [SerializeField] private float hitDistance = 0.15f;
    [SerializeField] private float returnDistance = 0.05f;

    private int attackDamage;
    private float coolTime;
    private float coolTimer;

    private FairyMovement fairyMovement;
    private Coroutine attackRoutine;
    private bool isAttacking;

    private void Awake()
    {
        fairyMovement = GetComponent<FairyMovement>();

        if (fairyData != null)
        {
            attackDamage = fairyData.Damage;
            coolTime = fairyData.CoolTime;
        }
        else
        {
            Debug.LogWarning("[FairyAttackController] fairyData가 할당되지 않았습니다!");
        }
    }

    private void Update()
    {
        if (coolTimer > 0f)
            coolTimer -= Time.deltaTime;
    }

    public void RequestAttack(MonsterHealth target)
    {
        if (isAttacking)
            return;

        if (coolTimer > 0f)
        {
            Debug.Log($"[FairyAttackController] 쿨타임 중 ({coolTimer:F1}초 남음)");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("[FairyAttackController] 공격 대상이 없습니다.");
            return;
        }

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(AttackRoutine(target));
    }

    private IEnumerator AttackRoutine(MonsterHealth target)
    {
        isAttacking = true;
        coolTimer = coolTime;

        Vector3 homePosition = transform.position;

        if (fairyMovement != null)
            fairyMovement.LockFollow();

        yield return MoveToTarget(target);

        if (target != null)
        {
            target.TakeDamage(attackDamage);
            Debug.Log($"[FairyAttackController] {target.gameObject.name} 공격 - 데미지: {attackDamage}");
        }

        yield return ReturnHome(homePosition);

        if (fairyMovement != null)
            fairyMovement.UnlockFollow();

        attackRoutine = null;
        isAttacking = false;
    }

    private IEnumerator MoveToTarget(MonsterHealth target)
    {
        while (target != null)
        {
            Vector3 targetPos = target.transform.position;
            if (fairyMovement != null)
                fairyMovement.FaceTarget(targetPos);

            Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, dashSpeed * Time.deltaTime);
            transform.position = newPos;

            if (Vector3.Distance(transform.position, targetPos) <= hitDistance)
            {
                if (fairyMovement != null)
                    fairyMovement.FaceTarget(targetPos);

                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator ReturnHome(Vector3 homePosition)
    {
        while (Vector3.Distance(transform.position, homePosition) > returnDistance)
        {
            if (fairyMovement != null)
                fairyMovement.FaceTarget(homePosition);

            transform.position = Vector3.MoveTowards(transform.position, homePosition, returnSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = homePosition;

        if (fairyMovement != null)
            fairyMovement.SyncFollowState(homePosition);
    }

    public bool CanAttack => !isAttacking && coolTimer <= 0f;
}
