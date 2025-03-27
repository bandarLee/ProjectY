using Photon.Pun;
using UnityEngine;
using Unity.Cinemachine;
public class PlayerPhotonSigleton : MonoBehaviour
{
    public static PlayerPhotonSigleton Instance;
    public Transform cameraRoot;
    public PhotonView PhotonView;
    public CinemachineCamera cCam;

    void Awake()
    {
        PhotonView = GetComponent<PhotonView>();

        // 이 오브젝트가 내 캐릭터일 경우에만 싱글턴 할당
        if (PhotonView.IsMine)
        {
            Instance = this;
        }
        else
        {
            // 내 캐릭터가 아니라면 해당 스크립트는 제거할 수도 있음
            Destroy(this);
        }
    }

    void Start()
    {
        if (PhotonView.IsMine)
        {
            cCam = FindObjectOfType<CinemachineCamera>();
            if (cCam != null)
            {
                cCam.Follow = cameraRoot;

            }
            else
            {
            }
        }
    }
}