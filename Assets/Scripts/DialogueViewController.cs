using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class DialogueViewController : MonoBehaviour
{
    public InputEventDispatcher inputDispatcher;
    public DialogueRunner dialogueRunner;
    public Text infoText;

    [Header("Dialogue Nodes")]
    public string startNode = "GameInit";
    public string boardNode = "OnLookAt_Board";
    public string friendNode = "OnTalk_Friend";
    public string windowNode = "OnLookAt_Window";
    public string deskNode = "OnLookAt_Desk";

    [Header("UI Text")]
    public string defaultInfo = "按方向键触发对话";
    public string boardInfo = "触发公告板对话";
    public string friendInfo = "触发朋友对话";
    public string windowInfo = "触发窗户对话";
    public string deskInfo = "触发书桌对话";

    void Start()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.StartDialogue(startNode);
        }

        UpdateInfo(defaultInfo);
    }

    void OnEnable()
    {
        if (inputDispatcher == null)
            return;

        inputDispatcher.onBoard.AddListener(OnBoardSelected);
        inputDispatcher.onFriend.AddListener(OnFriendSelected);
        inputDispatcher.onWindow.AddListener(OnWindowSelected);
        inputDispatcher.onDesk.AddListener(OnDeskSelected);
    }

    void OnDisable()
    {
        if (inputDispatcher == null)
            return;

        inputDispatcher.onBoard.RemoveListener(OnBoardSelected);
        inputDispatcher.onFriend.RemoveListener(OnFriendSelected);
        inputDispatcher.onWindow.RemoveListener(OnWindowSelected);
        inputDispatcher.onDesk.RemoveListener(OnDeskSelected);
    }

    void OnBoardSelected()
    {
        StartNode(boardNode);
        UpdateInfo(boardInfo);
    }

    void OnFriendSelected()
    {
        StartNode(friendNode);
        UpdateInfo(friendInfo);
    }

    void OnWindowSelected()
    {
        StartNode(windowNode);
        UpdateInfo(windowInfo);
    }

    void OnDeskSelected()
    {
        StartNode(deskNode);
        UpdateInfo(deskInfo);
    }

    void StartNode(string nodeName)
    {
        if (dialogueRunner == null)
        {
            Debug.LogWarning("DialogueRunner is not assigned.");
            return;
        }

        dialogueRunner.StartDialogue(nodeName);
    }

    void UpdateInfo(string message)
    {
        if (infoText != null)
        {
            infoText.text = message;
        }
    }
}
