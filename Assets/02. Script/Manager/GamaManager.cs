using Photon.Pun;
using UnityEngine;

public class GamaManager : MonoBehaviour
{
    public GameObject TestPlayerPrefab;
    void Start()
    {
        GameObject player = PhotonNetwork.Instantiate(TestPlayerPrefab.name, Vector3.zero, Quaternion.identity, 0);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
