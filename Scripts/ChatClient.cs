using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class ChatClient : MonoBehaviour
{

   
    public GameObject pauseMenu;
    public GameObject chatMenu;
    private GameObject chatView;
    public GameObject osd;
    public GameObject multiplayerWorlds;

    [SerializeField]
    private GameObject messageInput;
    [SerializeField]
    private GameObject recieverInput;


    [SerializeField]
    private GameObject mpInfoText;

    private PeerToPeerManager peerToPeerConnection; //TODO get connection

    private bool menuOpen = false;
    private int numOfDisplayedMessages = 8;
    Text[] messageFieldTexts;
    GameObject[] messageFields;

    private bool isMultiplayer = false;
    private string ownPlayerName;

    private List<string> playerNames = new List<string>();

    private List<ChatMessage> allChatMessages = new List<ChatMessage>();

    private string localIpAdress = "";

    public static ChatClient instance;


    public string GetLocalIPAddress()
    {
        if (localIpAdress.Equals(""))
        {
            var host = System.Net.Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIpAdress = ip.ToString();
                    return localIpAdress;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        return localIpAdress;
    }

    // Start is called before the first frame update
    void Start()        
    {
        instance = this;
        isMultiplayer = Scenes.getParam("multiplayer") == "true";      
        ownPlayerName = Scenes.getParam("userName");
        peerToPeerConnection = multiplayerWorlds.GetComponent<PeerToPeerManager>();  
        chatView = chatMenu.transform.GetChild(0).gameObject;
        messageFieldTexts = new Text[numOfDisplayedMessages];
        messageFields = new GameObject[numOfDisplayedMessages];


        mpInfoText.GetComponent<Text>().text = "Local IP Adress = "+GetLocalIPAddress()+"\n-------------------\n" +
            "Local Port = "+ Scenes.getParam("localPort"); ;

        chatMenu.SetActive(false);
        for (int i = 0; i < numOfDisplayedMessages; i++)
        {
            messageFields[numOfDisplayedMessages - 1 - i] = chatView.transform.GetChild(i).gameObject;
            messageFieldTexts[numOfDisplayedMessages-1-i] = messageFields[numOfDisplayedMessages - 1 - i].transform.GetChild(0).GetComponent<Text>();
        }
        if (isMultiplayer)
        {
            messageFieldTexts[0].text = "online!";
        }
        else
        {
            messageFieldTexts[0].text = "offline!";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isMultiplayer) {
            GetNewMessages();
        }
        
        if (menuOpen)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendMessage();              
            }
            
            UpdateUi();    
        }
    }

    public void ToggleChatMenu()
    {
        Debug.Log("input for chat client");
        if (menuOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }


    private void GetNewMessages()
    {
        //get the list from the tcp connection
        //read all elements 
        //a)write them to the local allChatMessages list
        //b)remove them from the original list
        //if anything changed refresh the displayed chat messages
        if (peerToPeerConnection.chatMessages.Count == 0)
            return;

        List<ChatMessage> newChatMessages = peerToPeerConnection.chatMessages;
        while (newChatMessages.Count > 0)
        {
            ChatMessage newMessage = newChatMessages[0];
            if (!playerNames.Contains(newMessage.sender) && !newMessage.sender.Equals("")){
                playerNames.Add(newMessage.sender);
            }
            allChatMessages.Add(newMessage);
            newChatMessages.Remove(newMessage);
        }
    }

    private void HandleCommand(string command)
    {
        if (command.ToLower().Contains("getstate"))
        {
            //peerToPeerConnection.AskForState();
            return;
        }
    }

    private void SendMessage()
    {       
        if (!messageInput.GetComponent<InputField>().text.Equals(""))
        {

            string messageContent = messageInput.GetComponent<InputField>().text;
            //check for command line
            var charArray = messageContent.ToCharArray();
            if (charArray[0] == '/')
            {
                Debug.Log("entered command!");
                HandleCommand(messageContent);

                return;
            }


            if(!recieverInput.GetComponent<InputField>().text.Equals(""))
            {

                //read text from textfield and from receiver field
                //create a new via the tcpconnection.sendChatMessage()
                Debug.Log("reading text: " + messageInput.GetComponent<InputField>().text);

               
                string messageReceiver = recieverInput.GetComponent<InputField>().text;
                messageInput.GetComponent<InputField>().text = "";
                Debug.Log("reading text after change: " + messageInput.GetComponent<InputField>().text);
                Debug.Log("sending message: " + messageContent + " to: " + messageReceiver);
                //TODO
                //peerToPeerConnection.SendChatMessage(messageReceiver, messageContent);
                SSTools.ShowMessage("Message Send succesfully!", SSTools.Position.bottom, SSTools.Time.oneSecond);
            }


        }
        else
            SSTools.ShowMessage("Please specify both receiver and content of the messeage!", SSTools.Position.bottom, SSTools.Time.threeSecond);
    }

    private void UpdateUi()
    {
        if (!isMultiplayer)
        {
            return;
        }
        if (allChatMessages.Count == 0)
        {
            return;
        }

        for (int i = 0; i < numOfDisplayedMessages; i++)
        {
            messageFieldTexts[i].text = allChatMessages[allChatMessages.Count -i -1].ToString();
            messageFields[i].GetComponent<Image>().color = new Color(255, 255, 255, 60);
            if (i + 1 >= allChatMessages.Count)
                return;
        }
    }


    public void OpenMenu()
    {
        menuOpen = true;
        chatMenu.SetActive(true);
        osd.SetActive(false);
    }
    public void CloseMenu()
    {
        menuOpen = false;
        chatMenu.SetActive(false);
        osd.SetActive(true);
    }
}
