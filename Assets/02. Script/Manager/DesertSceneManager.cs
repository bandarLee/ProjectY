using Photon.Pun;
using UnityEngine;

public class DesertSceneManager : MonoBehaviour
{
    public GameObject Player;
    public Vector3 PlayerStartPosition;
    void Start()
    {
        GameObject player = PhotonNetwork.Instantiate(Player.name, PlayerStartPosition, Quaternion.identity, 0);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
