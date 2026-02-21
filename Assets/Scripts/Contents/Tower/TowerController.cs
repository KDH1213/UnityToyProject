using System.Collections;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    [SerializeField]
    private TowerGround ownerGround;

    [SerializeField]
    private TowerProfile towerProfile;
    public TowerProfile TowerProfile { get { return towerProfile; } }

    [SerializeField]
    private TowerUIController uIController;

    [SerializeField]
    private int targetSize = 10;

    [SerializeField]
    private Transform targetTransform;
    private IDamageable targetInfo;

    [SerializeField]
    private AttackInfoData attackInfoData;

    [SerializeField]
    private CreateTowerData[] createTowerDatas;
    public CreateTowerData[] CreateTowerDatas { get { return createTowerDatas; } }

    private Vector3 ownerPosition;
    private Vector2 currentPosition;
  
    private Collider[] targetColliders;
    private Coroutine targetQuest = null;

    private bool isDrawInfo = false;

    private float questRange = 0.2f;

    private float currentTime = 0f;
    private bool isAttackTarget = false;
    private bool isTarget = false;
    private bool isReload = true;

    private void Start()
    {
        targetColliders = new Collider[targetSize];

        targetQuest = StartCoroutine(CoFindTarget());
    }

    public void SetCreateInfo(TowerGround owner)
    {
        ownerGround = owner;
        ownerGround.SetTower(this);

        ownerPosition = ownerGround.transform.position;
        currentPosition = new Vector2(ownerPosition.x, ownerPosition.z);
    }

    private bool OnFindTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(ownerPosition, towerProfile.AttackRange, targetColliders
            , GetLayerMasks.Enemy | GetLayerMasks.GroundEnemy | GetLayerMasks.AerialEnemy);

        if (count == 0)
            return false;

        float currentDistance = float.MaxValue;
        Vector2 target;

        for(int i = 0; i < count; ++i)
        {
            target = new Vector2(targetColliders[i].transform.position.x, targetColliders[i].transform.position.z);
            float targetDistance = Vector2.Distance(target, currentPosition);
            if (currentDistance > targetDistance)
            {
                if (targetColliders[i].transform.GetComponent<IDamageable>().IsDead)
                    continue;

                isTarget = true;
                targetTransform = targetColliders[i].transform;
                currentDistance = targetDistance;
            }
        }

        return isTarget;
    }

    protected virtual void CreateAttackObject()
    {
        var createObject = Instantiate(attackInfoData.AttackObjectPrefab);
        createObject.transform.position = transform.position + attackInfoData.CreateOffsetPos;

        var dagedObject = createObject.GetComponent<ProjectTile>();
        dagedObject.TargetShooting(transform, targetTransform, OnAttackTarget);

        // targetTransform.position;
    }

    private void OnAttackTarget()
    {
        //if(targetTransform.GetComponent<IDamageable>().IsDead)
        //{
        //    isTarget = false;
        //    targetTransform = null;
        //    targetQuest = StartCoroutine(CoFindTarget());
        //}
    }

    private void Update()
    {
        if (!isTarget)
        {
            if (targetQuest == null)
            {
                targetQuest = StartCoroutine(CoFindTarget());
            }
        }
        else
        {
            isTarget = CheckTargetStatus();
            AttackCheck();
        }
    }

    private bool CheckTargetStatus()
    {
        if (targetTransform == null)
            return false;

        Vector2 target = new Vector2(targetTransform.position.x, targetTransform.position.z);
        if (targetInfo.IsDead || towerProfile.AttackRange < Vector2.Distance(target, currentPosition))
        {
            targetTransform = null;
            targetInfo = null;
            isTarget = false;
            return false;
        }

        return true;
    }

    private void AttackCheck()
    {
        if (isReload)
        {
            if (!isTarget)
                return;

            CreateAttackObject();
            isReload = false;
        }
        else
        {
            currentTime += GameTimeManager.Instance.DeltaTime;
            if (currentTime >= towerProfile.AttackSpeed)
            {
                currentTime = 0f;
                isReload = true;
            }
        }
    }

    private IEnumerator CoFindTarget()
    {
        bool isQuest = false;
        while (!isQuest)
        {
            yield return new WaitForEndOfFrame();
            isQuest = OnFindTarget();
        }

        targetInfo = targetTransform.GetComponent<IDamageable>();
        targetQuest = null;
    }

    public void OnDrawInfo()
    {
        isDrawInfo = true;
        uIController.OnDrawInfo();
    }

    public void offDrawInfo()
    {
        isDrawInfo = true;
        uIController.OffDrawInfo();
    }

    private void OnDrawGizmosSelected()
    {
        if (isDrawInfo)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(ownerPosition, towerProfile.AttackRange);
        }
    }
}