using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class LobbyManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";

    public Button CreateButton;
    public Button joinButton;
    public GameObject BaseUI;
    public GameObject CreateUI;
    public GameObject JoinUI;

    public TMP_InputField RoomNameInput;
    public TMP_InputField NickNameInputCreate;
    public TMP_InputField NickNameInputJoin;

    public Transform roomListContainer; 
    public GameObject roomButtonPrefab;

    private Dictionary<string, GameObject> roomButtonDict = new Dictionary<string, GameObject>();


    private void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        CreateUI.SetActive(false);
        JoinUI.SetActive(false);
        CreateButton.interactable = false;
        joinButton.interactable = false;
    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        CreateButton.interactable = true;
        joinButton.interactable = true;
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        CreateButton.interactable = false;
        joinButton.interactable = false;
        PhotonNetwork.ConnectUsingSettings();
    }
    public void Connect()
    {
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;
        PhotonNetwork.PhotonServerSettings.DevRegion = "kr";
        PhotonNetwork.AutomaticallySyncScene = false;
        if (PhotonNetwork.IsConnected)
        {

            RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
            PhotonNetwork.JoinOrCreateRoom("Server1", roomOptions, TypedLobby.Default);
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined Room");
        StartLoading();

    }
    public void StartLoading()
    {

        PhotonNetwork.LoadLevel("02Desert");

    }

    public void RoomCreateUI()
    {
        SetActiveScreen(1);
    }
    public void RoomJoinUI()
    {
        SetActiveScreen(2);
    }
    public void BackSpaceUI()
    {
        SetActiveScreen(0);
    }
    public void CreateRoom()
    {
        Debug.Log("방 생성 버튼 클릭.");

        string roomName = RoomNameInput.text;
        string nickName = NickNameInputCreate.text;

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.Log("방 이름이 비어있습니다.");
            return;
        }
        if (string.IsNullOrEmpty(nickName))
        {
            Debug.Log("닉네임이 비어있습니다.");
            return;
        }

        PhotonNetwork.NickName = nickName;
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 20 };
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
   
    public void JoinRoom(string roomName)
    {
        string nickName = NickNameInputJoin.text;
        if (string.IsNullOrEmpty(nickName))
        {
            Debug.Log("닉네임이 비어있습니다.");
            return;
        }

        PhotonNetwork.NickName = nickName;
        PhotonNetwork.JoinRoom(roomName);
    }
    public void SetActiveScreen(int ScreenNumber)
    {
        switch (ScreenNumber)
        {
            case 0: //로비로 뒤로가기
                CreateUI.SetActive(false);
                JoinUI.SetActive(false);
                BaseUI.SetActive(true);
                break;
            case 1: // 방 생성
                CreateUI.SetActive(true);
                JoinUI.SetActive(false);
                BaseUI.SetActive(false);
                break;
            case 2: // 방 참가
                CreateUI.SetActive(false);
                JoinUI.SetActive(true);
                BaseUI.SetActive(false);
                break;


        }
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                if (roomButtonDict.ContainsKey(room.Name))
                {
                    Destroy(roomButtonDict[room.Name]);
                    roomButtonDict.Remove(room.Name);
                }
            }
            else
            {
                if (!roomButtonDict.ContainsKey(room.Name))
                {
                    GameObject roomButton = Instantiate(roomButtonPrefab, roomListContainer);
                    TMP_Text buttonText = roomButton.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";
                    }

                    Button button = roomButton.GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.AddListener(() => { JoinRoom(room.Name); });
                    }
                    roomButtonDict.Add(room.Name, roomButton);
                }
                else
                {
                    GameObject roomButton = roomButtonDict[room.Name];
                    TMP_Text buttonText = roomButton.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";
                    }
                }
            }
        }
    }
}


