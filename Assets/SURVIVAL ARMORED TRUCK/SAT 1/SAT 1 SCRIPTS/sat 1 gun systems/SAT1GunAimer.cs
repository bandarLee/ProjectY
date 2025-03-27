using UnityEngine;
using UnityEngine.UI;

namespace SAT1GunAimer
{

public class SAT1GunAimer : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private Transform gunPivot;
    [SerializeField] private Transform carBody;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float returnSpeed = 2f;

    [Header("X-Axis Limits")]
    [SerializeField] private float minXRotation = -20f;
    [SerializeField] private float maxXRotation = 45f;

    [Header("Aim Assist Settings")]
    [SerializeField] private Image aimAssistUI;
    [SerializeField] private float aimAssistRange = 50f;
    [SerializeField] private Color lockedOnColor = Color.red; // User can choose Red or Green
    [SerializeField] private Color defaultColor = Color.white;
    
    private bool useAimAssist = false;
    private Transform targetEnemy;
    private Quaternion defaultLocalRotation;

    void Start()
    {
        defaultLocalRotation = gunPivot.localRotation;
        aimAssistUI.color = defaultColor; // Ensure UI starts with default color
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Right-click to switch aim mode
        {
            useAimAssist = !useAimAssist;
            if (useAimAssist)
            {
                ResetGunToForward();
            }
            else
            {
                aimAssistUI.color = defaultColor; // Reset UI color when switching back
            }
        }

        if (useAimAssist)
        {
            FindTargetWithUI();
        }
        else
        {
            FindTarget();
            RotateGun();
        }
    }

    void FindTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.transform;
            }
        }

        targetEnemy = closestEnemy;
    }

    void FindTargetWithUI()
    {
        targetEnemy = null;

        Ray ray = Camera.main.ScreenPointToRay(aimAssistUI.transform.position);
        if (Physics.Raycast(ray, out RaycastHit hit, aimAssistRange, enemyLayer))
        {
            targetEnemy = hit.transform;
            aimAssistUI.color = lockedOnColor; // Change color when locked on
        }
        else
        {
            aimAssistUI.color = defaultColor; // Reset to default if no enemy is locked
        }
    }

    void RotateGun()
    {
        if (targetEnemy == null)
        {
            gunPivot.localRotation = Quaternion.Lerp(gunPivot.localRotation, defaultLocalRotation, Time.deltaTime * returnSpeed);
            return;
        }

        Vector3 directionToEnemy = (targetEnemy.position - gunPivot.position).normalized;
        Vector3 localDirection = gunPivot.parent.InverseTransformDirection(directionToEnemy);

        float yRotation = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        float xRotation = -Mathf.Asin(localDirection.y) * Mathf.Rad2Deg;
        xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);

        Quaternion targetRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        gunPivot.localRotation = Quaternion.Lerp(gunPivot.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    void ResetGunToForward()
    {
        StartCoroutine(SmoothReset());
    }

    System.Collections.IEnumerator SmoothReset()
    {
        Quaternion startRotation = gunPivot.localRotation;
        float elapsedTime = 0f;
        float duration = 0.5f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            gunPivot.localRotation = Quaternion.Lerp(startRotation, defaultLocalRotation, elapsedTime / duration);
            yield return null;
        }

        gunPivot.localRotation = defaultLocalRotation;
    }

    public Transform GetTargetEnemy()
    {
        return targetEnemy;
    }
}
}