using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TargetFollowProjectTileController : ProjectTileController
{
    [SerializeField]
    private bool isTargetFollow;

    [SerializeField]
    private bool isOnlyTargetHit;

    [SerializeField]
    private Transform targetTransform;

    [SerializeField]
    private float moveTime;
    Vector3 endPoint;
    private float currentTime;

    protected override void Update()
    {
        currentTime += GameTimeManager.Instance.DeltaTime;
        transform.position = Vector3.Lerp(startPos,endPoint , currentTime / moveTime);
        if(currentTime >= moveTime)
        {
            hitEvent?.Invoke();
            Destroy(gameObject);
        }
    }

    public void TargetShooting(Transform target,UnityAction _hitEvent = null, UnityAction _destoryEvent = null)
    {
        targetTransform = target;

        int targetLayer = targetTransform.gameObject.layer;

        SetEvent(_hitEvent, _destoryEvent);
    }

    private void DisconnectAsTargetDestroy()
    {
        isTargetFollow = false;
        endPoint = targetTransform.position;
        moveDirection = Vector3.Normalize(endPoint - rigidbody.position);

        endMovementDistance = Vector3.Distance(targetTransform.position, rigidbody.position);

        

        if (moveSpeed <= 0f)
            moveSpeed = 10f;
    }
}
