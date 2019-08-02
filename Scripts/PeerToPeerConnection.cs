using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


using System.Collections.Generic;
using System.Linq;

abstract public class PeerToPeerConnection
{
   

    protected int port;

    protected PeerToPeerManager peerToPeerManager;

    public TcpClient socketConnection;
    public Thread clientReceiveThread;

    private string bufferedMessage = "";

    public string clientName = "";

    public PeerToPeerConnection(PeerToPeerManager managerInstance)
    {
        this.peerToPeerManager = managerInstance;
    }


   

    abstract public void SendToPeer(string messsage);

  
  
    public void SendGetPeers()
    {
        Debug.Log("P2PConnection: send request for peers!");
        CreateAndSendMessage(new GetPeersMessage());
    }

    public void SendGetState()
    {
        Debug.Log("P2PConnection: send get state");
        CreateAndSendMessage(new GetStateMessage(peerToPeerManager.myClientName));
    }


    private Vector3 ConvertStringToVector3(string input)
    {
        string[] tokens = input.Split(',');
        int[] coordinates = Array.ConvertAll<string, int>(tokens, int.Parse);
        return new Vector3(coordinates[0], coordinates[1], coordinates[2]);
    }

    public void OnApplicationQuit()
    {
        try
        {
            socketConnection.GetStream().Close();
            socketConnection.Close();
            clientReceiveThread.Abort();
        }
        catch (Exception e)
        {
            Debug.Log("P2PConnection: Cant close socketConnection: " + e.ToString());
        }
    }

    private List<string> splitMessage(string message)
    {
        //TODO
        return null;
    }

    private string mergeMessages(string messageA, string messageB)
    {
        //TODO
        return null;
    }

    private List<string> BufferMessage(string message)
    {
        Debug.Log("Buffering Message");
        List<string> bufferedMessages = new List<string>();
        bool done = false;
        int bufferCounter = 1;
        while (!done)
        {
            bufferedMessage += message;
            Debug.Log("buffering " + (bufferCounter++) + " -> " + bufferedMessage);

            int index = 0;
            int ratio = 0;
            int opened = 0;
            //serching one message
            while (index < bufferedMessage.Length)
            {
                char c = bufferedMessage[index];
                if (c == '{')
                {
                    opened++;
                    ratio++;
                }
                else if (c == '}')
                {
                    ratio--;
                }
                if (opened > 0 && ratio == 0)
                {
                    //then one message is complete
                    //if not breaked (no message end found) index would now be equal and not smaler to....
                    string firstMessage = bufferedMessage.Substring(0, index+1);
                    bufferedMessages.Add(firstMessage);

                    if (bufferedMessage == "" || index >= bufferedMessage.Length - 1)
                    {
                        done = true;
                        bufferedMessage = "";
                    }
                    else
                    {
                        string rest = bufferedMessage.Substring(index, bufferedMessage.Length - index);
                        if (rest[0] == '{')
                        {
                            bufferedMessage = rest;
                            Debug.Log("rest == " + rest);
                        }
                        else
                        {
                            bufferedMessage = "";
                        }
                    }
                    Debug.Log("breaked out of inner loop -> found one message!");
                    break;
                }
                index++;
            }
            Debug.Log("ran out of inner loop -> no message found");
            if (bufferedMessage == "" || index >= bufferedMessage.Length - 1)  
            {
                //need to add the message to the bufferedMessages
                //no more messages found!
                done = true;
            }
        }
        Debug.Log("buffered " + bufferedMessages.Count + " messages");
        return bufferedMessages;
    } 
    /**
     * A Mesasge can only be valid of all open tags are followed by a close tag (count equal) and the first and last chars are tags
     * 
     */ 
    private bool MessageIsValid(string message)
    {
        char fistChar = message[0];
        if (fistChar != '{')
            return false;
        char lastChar = message[message.Length - 1];
        if (lastChar != '}')
            return false;
        int countOpen = message.Count(f => f == '{');
        int countClose = message.Count(f => f == '}');
        if (countOpen != countClose)
            return false;
        return true;
    }



    public void HandleIncommingMessage(string message)
    {
        Debug.Log("P2PConnection: Handling incomming Message: " + message);
        if (!MessageIsValid(message))
        {
            List<string> bufferendMessages = BufferMessage(message);
            foreach(string bufferedMessage in bufferendMessages)
            {
                HandleMessage(bufferedMessage);
            }
        }
        else
        {
            HandleMessage(message);
        }
    }



    private void HandleMessage(string message)
    {
        Debug.Log("P2PConnection: Handling new Message: " + message);

        if (message.Contains("\"type\":\"ping\""))
        {
            HandlePingMessage(message);
            return;
        }

        if (message.Contains("\"type\":\"addBlock\""))
        {
            HandleAddBlockMessage(message);
            return;
        }
        if (message.Contains("\"type\":\"deleteBlock\""))
        {
            HandleDeleteBlockMessage(message);
            return;
        }
        if (message.Contains("\"type\":\"chat\""))
        {
            HandleChatMessage(message);
            return;
        }

        if (message.Contains("\"type\":\"hello\""))
        {
            HandleHelloMessage(message);
            return;
        }

        if (message.Contains("\"type\":\"peers\""))
        {
            HandlePeersMessage(message);
            return;
        }

        if (message.Contains("\"type\":\"getPeers\""))
        {
            HandleGetPeersMessage(message);
            return;
        }

        if (message.Contains("\"type\":\"position\""))
        {
            HandlePositionMessage(message);
            return;
        }

        if (message.Contains("\"type\":\"state\""))
        {
            HandleStateMessage(message);
            return;
        }

        if (message.Contains("\"type\":\"getState\""))
        {
            HandleGetStateMessage(message);
            return;
        }
                     
        if (message.Contains("\"type\":\"welcome\""))
        {
            //TODO needed???
            //WelcomeMessage welcomeMessage = JsonUtility.FromJson<WelcomeMessage>(message);
            //playerMove.moveTo(ConvertStringToVector3(welcomeMessage.position));
            Debug.LogWarning("P2PConnection: Handle incomming welcome Message not definded!");
            return;
        }
                              

        if (message.Contains("\"type\":\"end\""))
        {
            Debug.Log("P2PConnection: Received End Message - shutdown netork connetion!");
            clientReceiveThread.Abort();
            //socketConnection.Close();
            Debug.Log("P2PConnection: connetion closed!");
            //TODO just terminate the connection! -> no object needed
            return;
        }
        else
        {
            Debug.LogWarning("P2PConnection: Undefined Message Type in Message: " + message);
        }
    }

    private void HandlePingMessage(string message)
    {
        PingMessage pingMessage = JsonUtility.FromJson<PingMessage>(message);
        
        if (!pingMessage.isResponse)
        {
            pingMessage.Respond();
            CreateAndSendMessage(pingMessage);
        }
        else
        {
            pingMessage.Received();
            peerToPeerManager.AddIncommingMessage(pingMessage);
        }
    }

    private void HandlePositionMessage(string message)
    {
        /*
        JObject jObj = JObject.Parse(message);
        JToken nameAsJToken = jObj.GetValue("name");
        JToken posAsJToken = jObj.GetValue("position");
        string name = JsonConvert.DeserializeObject<string>(nameAsJToken.ToString());
        Vector3 position = JsonConvert.DeserializeObject<Vector3>(posAsJToken.ToString());
        */
        //PositionMessage positionMessage = new PositionMessage(position, name);
        Debug.Log("handle position message: " + message);
        PositionMessage positionMessage = JsonUtility.FromJson<PositionMessage>(message);
        peerToPeerManager.AddIncommingMessage(positionMessage);
        
    }

    private void HandleGetPeersMessage(string message)
    {
        Debug.Log("P2PConnection: handle get peers message");
        PeersMessage peersMessage = new PeersMessage(peerToPeerManager.peers);
        Debug.Log("# of known peers = " + peerToPeerManager.peers.Count);
        CreateAndSendMessage(peersMessage);
        Debug.LogWarning("P2PConnection: message sent!");
    }

    private void HandlePeersMessage(string peersMessage)
    {
        Debug.LogWarning("P2PConnection: handle peers message");

        //PeersMessage peersMessage = JsonUtility.FromJson<PeersMessage>(message);


        JObject jObj = JObject.Parse(peersMessage);
        JToken peersToken = jObj.GetValue("peers");

        PeersMessage peersMessageInstance = new PeersMessage(JsonConvert.DeserializeObject<Dictionary<string, string>>(peersToken.ToString()));
        Debug.Log("P2PConnection: peers message content = " + peersMessageInstance.ToString());

        peerToPeerManager.AddIncommingMessage(peersMessageInstance);





        //peerToPeerManager.AddIncommingMessage(peersMessage);
    }


    private void HandleHelloMessage(string message)
    {
        Debug.Log("P2PConnection: Handling hello message");        
        HelloMessage helloMessage = JsonUtility.FromJson<HelloMessage>(message);
        peerToPeerManager.AddHelloMesssage(helloMessage, this);
        this.clientName = helloMessage.name;
        /*
        JObject jObj = JObject.Parse(message);
        JToken nameAsJToken = jObj.GetValue("name");
        string name = JsonConvert.DeserializeObject<string>(nameAsJToken.ToString());
        peerToPeerManager.AddConnection(name, this);
        */

        //1. save new client with name (by adding this message to the manager)
        //2. respond to peer with position
    }

    private void HandleAddBlockMessage(string message)
    {
        Debug.LogWarning("P2PConnection: handle add block message");
        
        AddBlockMessage addBlockMessage = JsonUtility.FromJson<AddBlockMessage>(message);
        peerToPeerManager.AddIncommingMessage(addBlockMessage);
        
    }

    private void HandleDeleteBlockMessage(string message)
    {
        Debug.LogWarning("P2PConnection: handle delete block message");
        
        DeleteBlockMessage deleteBlockMessage = JsonUtility.FromJson<DeleteBlockMessage>(message);
        peerToPeerManager.AddIncommingMessage(deleteBlockMessage);
 
    }

    private void HandleChatMessage(string messageString)
    {
        Debug.Log("P2PConnection: Receiving Chat Message! " + messageString);
        //TODO deserialize does not work
        ChatMessage chatMessage = JsonUtility.FromJson<ChatMessage>(messageString);
        //chatMessage.SetDate();
        peerToPeerManager.AddIncommingMessage(chatMessage);
        Debug.Log("P2PConnection: Message content = " + chatMessage);
    }
    private void HandleGetStateMessage(string message)
    {
        GetStateMessage getStateMessage = JsonUtility.FromJson<GetStateMessage>(message);
        peerToPeerManager.AddIncommingMessage(getStateMessage);
       

       
    }

    private void HandleStateMessage(string stateMessage)
    {
        Debug.Log("P2PConnection: Handling state msessage!" + stateMessage);
        JObject jObj = JObject.Parse(stateMessage);
        JToken blockPositions = jObj.GetValue("blockPositions");
        JToken playerPositions = jObj.GetValue("playerPositions");
        Debug.Log("parsing block positions " + blockPositions.ToString());
        Dictionary<string, int> dictionaryBlockPositions = JsonConvert.DeserializeObject<Dictionary<string, int>>(blockPositions.ToString());
        Debug.Log("parsing player positions " + playerPositions.ToString());
        Dictionary<string, string> dictionaryPlayerPositions = JsonConvert.DeserializeObject<Dictionary<string, string>>(playerPositions.ToString());

        Dictionary<Vector3, int> actualBlocks = new Dictionary<Vector3, int>();

        Debug.Log("Serialization done!");
            
        //StateMessage stateMessageInstance = JsonUtility.FromJson<StateMessage>(stateMessage);
        StateMessage stateMessageInstance = new StateMessage(dictionaryPlayerPositions, dictionaryBlockPositions);
        Debug.Log("P2PConnection: State message content = " + stateMessageInstance.ToString());

        peerToPeerManager.AddIncommingMessage(stateMessageInstance);

        //JArray a = JArray.Parse(stateMessageObject.GetValue("positions").ToString());
        //Debug.Log(a.ToString());

        //TODO handle state! -> push data to Multiplayer System
    }

    /*

1.1.4 Chat-Nachricht(Server -¿ Client)
• Key = ”type”, Value = ”chat”
• Key = ”sender”, Value = ”Name des Senders”
• Key = ”content”, Value = ”Textnachricht”
• Key = ”world”, Value = ”true”|”false”
• Beispiel: { "type" : "chat", "sender" : "me", "content" : "Hello
cube", "world" : "false" }

    */
    public void CreateAndSendMessage(Message messageObj)
    {
        Debug.Log("P2PConnection: create " + messageObj.ToString() + "!");
        var setting = new JsonSerializerSettings();
        //setting.Formatting = Formatting.Indented;
        setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        string outgoingMessage = JsonConvert.SerializeObject(messageObj,setting);
        //string outgoingMessage = JsonUtility.ToJson(messageObj);
        SendToPeer(outgoingMessage);
    }


}
