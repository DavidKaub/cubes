using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class PeerToPeerManager : MonoBehaviour
{

    private BuildingSystem buildingSystem;

    [SerializeField]
    private GameObject blockManager;

    [SerializeField]
    private GameObject textFieldPlayerPings;

    private MultiplayerSystem multiplayerSystem;

    public string myClientName;

    [SerializeField]
    private GameObject player;
    private PlayerMove playerMove;


    public Vector3 playersPosition;

    private bool readingMessages = true;

    private bool isConnected = false;
    private bool isRegistred = false;


    private string ipOfFirstPeerConnecton;
    public int localPort;
    private int remotePort;
    private string localIpAdress = "";



    private PeerToPeerConnection connectionToFirstPeer = null;



    public List<ChatMessage> chatMessages = new List<ChatMessage>();
    /*
     * used to establish connections
     *<playerName -> ip:port>
     */

    public Dictionary<string, string> peers = new Dictionary<string, string>();

    public Dictionary<Vector3, int> blockList = new Dictionary<Vector3, int>(); // <position, textur>


    public Dictionary<string, PingMessage> pings = new Dictionary<string, PingMessage>(); // <position, textur>

    public Dictionary<string, MutliplayerObject> players = new Dictionary<string, MutliplayerObject>(); // <playerName, Gameobject> -> used to communicate positions to others and to update the positions of its own representaions of other players
    private List<string> playersToKill = new List<string>();
    private Dictionary<string, PeerToPeerConnection> sockets = new Dictionary<string, PeerToPeerConnection>();
    List<Message> incommingMessages = new List<Message>();


    private Thread pingThread;

    /*
     * used to send messages
     *<playerName -> peerToPeerConnection>
     */


    private void HandlePingMessage(PingMessage pingMessage)
    {
        pings[pingMessage.receiver] = pingMessage;
    }



    private PeerToPeerServer server;


    public void DisplayPeersAndPings()
    {
        // send ping message in different thread? done
        //calculate mva? nope
        if (pings.Count == 0)
        {
            textFieldPlayerPings.GetComponent<Text>().text = "";
            return;
        }
        StringBuilder sb = new StringBuilder(300);        
        long maxPing = 0;
        try
        {
            foreach (KeyValuePair<string, PingMessage> entry in pings)
            {
                sb.Append(entry.Key);
                sb.Append(" - ");
                sb.Append(peers[entry.Key]);
                sb.Append(" = ");
                sb.Append(entry.Value.ping);
                sb.Append(" ms\n\n");
                if (entry.Value.ping > maxPing)
                {
                    maxPing = entry.Value.ping;
                }
            }
            sb.Append("Maximum Ping = ");
            sb.Append(maxPing);
            textFieldPlayerPings.GetComponent<Text>().text = sb.ToString();



        }
        catch(KeyNotFoundException knfe)
        {
            Debug.Log("Manager ping - key not found: " + knfe);
        }
        catch (Exception e)
        {
            Debug.Log("Manager ping - exception: " + e);
        }
     

            //calculate max while displaying
            //display mva of individuals
            //display max 
    }

    private void SendPingMessages()
    {
        Thread.Sleep(10000);
        while (true)
        {
            try
            {
                Thread.Sleep(1000);
                foreach (KeyValuePair<string, PeerToPeerConnection> entry in sockets)
                {
                    PingMessage ping = new PingMessage(entry.Key);
                    Debug.Log(myClientName + ": sending ping message to " + entry.Key);
                    entry.Value.CreateAndSendMessage(ping);
                }
            }
            catch(KeyNotFoundException keynotfound)
            {
                Debug.Log("manager can't send ping: " + keynotfound);
            }
            catch(Exception e)
            {
                Debug.Log("manager can't send ping (other exception): " + e);
            }          
        }
    }

   


    private void HandleRemoveClient()
    {
        foreach (string clientName in playersToKill)
        {
            if (players.ContainsKey(clientName)){

                Destroy(players[clientName].goInstance);
            }

            if (players.ContainsKey(clientName))
            {
                players.Remove(clientName);
            }
            if (peers.ContainsKey(clientName))
            {
                peers.Remove(clientName);
            }
            if (sockets.ContainsKey(clientName))
            {                
                sockets[clientName].socketConnection.Close();  
                sockets[clientName].clientReceiveThread.Abort();
                sockets.Remove(clientName);
            }
            if (pings.ContainsKey(clientName))
            {
                pings.Remove(clientName);
            }
        }
        playersToKill.Clear();
        DisplayPeersAndPings();

    }


        public void RemoveClient(string clientName)
        {
        playersToKill.Add(clientName);
         }

    public void AddBlockFromBuildingSystem(Vector3 pos ,int selectID)
    {
        blockList[pos] = selectID;
        AddBlockMessage message = new AddBlockMessage(pos, selectID);
        foreach (KeyValuePair<string, PeerToPeerConnection> entry in sockets)
        {
            entry.Value.CreateAndSendMessage(message);
        }
    }

    public void DeleteBlockFromBuildingSystem(Vector3 pos) {
        if (blockList.ContainsKey(pos)){
            blockList.Remove(pos);
            DeleteBlockMessage message = new DeleteBlockMessage(pos);
            foreach (KeyValuePair<string, PeerToPeerConnection> entry in sockets)
            {
                entry.Value.CreateAndSendMessage(message);
            }
        }else{
            Debug.Log("Manager of "+ myClientName+ ": i cannot delete a block which i never had!!" + pos);
        }

    }

    public void UpdateLocalPlayerPosition(Vector3 position)
    {
        PositionMessage message = new PositionMessage(position, myClientName);
        Debug.Log("Manager of "+myClientName+": Sending position message: "+ position + " " + myClientName + " to " + sockets.Count + " sockets!");
        foreach (KeyValuePair<string, PeerToPeerConnection> entry in sockets)
        {
            entry.Value.CreateAndSendMessage(message);
        }
    }



    void OnApplicationQuit()
    {
        pingThread.Abort();
        Debug.Log("Manager of "+myClientName+": Closing all sockets.. Shutting down");
        try

        {

            foreach (KeyValuePair<string, PeerToPeerConnection> entry in sockets)
            {
                entry.Value.OnApplicationQuit();
                server.OnApplicationQuit();
            }
        }
        catch (Exception e)
        {
            Debug.Log("Server: Cant stop tcpListener!!");
        }
    }




    void Start()
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
        bool isMultiplayer = Scenes.getParam("multiplayer") == "true";
        if (isMultiplayer)
        {
            playerMove = player.GetComponent<PlayerMove>();
            buildingSystem = blockManager.GetComponent<BuildingSystem>();
            multiplayerSystem = GetComponent<MultiplayerSystem>();
            myClientName = Scenes.getParam("userName");
            ipOfFirstPeerConnecton = Scenes.getParam("ip");

            if (ipOfFirstPeerConnecton.Equals("localhost"))
            {
                ipOfFirstPeerConnecton = GetLocalIPAddress();
            }

            string remotePortAsString = Scenes.getParam("remotePort");
            string localPortAsString = Scenes.getParam("localPort");

            remotePort = Int32.Parse(remotePortAsString);
            localPort = Int32.Parse(localPortAsString);

            Debug.Log("Manager of " + myClientName + ": Manager: IS Multiplayer! read ip and port from params = " + ipOfFirstPeerConnecton + ":" + remotePort + " / "+localPort);

            //players.Add(myClientName, player);

            //1 & 1b
            server = new PeerToPeerServer(this, localPort);
            server.Start();
            Debug.Log("Manager of " + myClientName + ": Manager: Server started");

            //1a)
            peers.Add(myClientName, GetLocalIPAddress()+":"+localPort);


            //2
            if((!ipOfFirstPeerConnecton.Equals("localhost")) && (!ipOfFirstPeerConnecton.Equals("127.0.0.1")) && (!ipOfFirstPeerConnecton.Equals(GetLocalIPAddress())))
            {

                Debug.Log("Manager of " + myClientName + ": connecting to peer of non local game");
                //then connect to peer
                ConnectToExistingPeers();

            }else if(remotePort != localPort)
            {

                Debug.Log("Manager of " + myClientName + ": connecting to ppeer of local game");
                ConnectToExistingPeers();
            }
            else
            {
                Debug.Log("Manager of " + myClientName + ": first peer - no connection needed");
            }


            /*
         * 1. eigenen Server starten (angabe des port)
         * a) speichern der eignen instanz in der liste der ip adressen um diese ggf an andere clients zu kommunizieren
         * b) Hören auf eingehende Verbindungen
         *           
         */
        }
        pingThread = new Thread(new ThreadStart(SendPingMessages));
        pingThread.IsBackground = true;
        pingThread.Start();
    }


    private void ConnectToExistingPeers()
    {
        Debug.Log("Manager of "+myClientName+": connect to existing peer: "+ipOfFirstPeerConnecton + " at port = "+remotePort);
        //verbindung zu einem peer
        connectionToFirstPeer = new PeerToPeerClient(this, ipOfFirstPeerConnecton,remotePort,"");


        


        //2.zu zuvor definierten ip verbindung aufbauen(remote server)

        //get ips and ports of all servers

        Thread.Sleep(1000);
        Debug.Log("should send message get Peers now!");
        connectionToFirstPeer.SendGetPeers();
    }


    private bool WaitForConnection(PeerToPeerConnection client)
    {
        int secondsWaiting = 0;
        int secondsWaitLimit = 10;
        while (false) // !is.connected()
        {
            Thread.Sleep(1000);
            secondsWaiting++;
            if (secondsWaiting > 1)
            {
                Debug.LogWarning("Manager of "+myClientName+": Still waiting for connection of first client (since " + secondsWaiting + " Seconds");
            }
            if (secondsWaiting > secondsWaitLimit)
            {
                Debug.LogWarning("Manager of "+myClientName+": stopped waiting after " + secondsWaiting + " Seconds!!!");
                return false;
            }
        }
        return true;
    }

    private void HandleAddBlock (AddBlockMessage messageToAdd)
    {
        Vector3 position = Message.ConvertStringToVector3(messageToAdd.position);
        int texture = messageToAdd.texture;
        if (blockList.ContainsKey(position))
        {
            blockList.Remove(position);
        }
        blockList.Add(position, texture);
        // TODO now tell buildingsystem to build block? 
        buildingSystem.PlaceBlockByPos(position, texture);
        Debug.Log("Manager of "+myClientName+": Someone added one block at position: " + position.ToString() +" and texture: " + texture);

    }

    private void HandleDeleteBlock (DeleteBlockMessage deleteMessage)
    {
        Vector3 position = Message.ConvertStringToVector3(deleteMessage.position);
        blockList.Remove(position);

        //TODO now tell buildingsystem to delete block
        buildingSystem.RemoveBlockByPosition(position);
        Debug.Log("Manager of "+myClientName+": Someone deleted the block at position: " + position.ToString());
    }

    private void HandlePeersMessage(PeersMessage peersMessage)
    {
        Debug.Log("handling peers message!");
        //parsed message should contain a dictionary 
        //iteratate through it and send hello message to each node
    if(peersMessage.peers == null)
        {
            Debug.Log("Manager of "+myClientName+": Received empty peers message");
            return;
        }

        foreach (KeyValuePair<string, string> entry in peersMessage.peers)
        {
            string playerName = entry.Key;
            string ipAndPort = entry.Value;

            string[] splits = ipAndPort.Split(':');
            string ip = splits[0];
            int port = Int32.Parse(splits[1]);

            Debug.Log("received ip = " + ip + " first ip = " + ipOfFirstPeerConnecton + " received port = " + port + " fist port = " + remotePort);

            if(ip.Equals(ipOfFirstPeerConnecton) && port == remotePort)
            {
                sockets.Add(playerName, connectionToFirstPeer);
                peers.Add(playerName, ipAndPort);
                connectionToFirstPeer.clientName = playerName;
                Debug.Log("added first peer to dictionaries");
                connectionToFirstPeer = null;
            }
            else
            { 
                if (!peers.ContainsKey(playerName))
                {
                    PeerToPeerClient anotherClient = new PeerToPeerClient(this, ip, port, playerName);
                    sockets.Add(playerName, anotherClient);
                    peers.Add(playerName, ipAndPort);
                    Debug.Log("Manager of "+myClientName+": Added new Player to known peers: " + playerName + " - " + ipAndPort);
                }
            }
           
            // do something with entry.Value or entry.Key
        }
        //now send hello message
        Debug.Log("Manager of " + myClientName + ": sending hello message known peers: "+ string.Join(";", peers));
        HelloMessage helloMessage = new HelloMessage(myClientName, playersPosition, peers[myClientName]);
        Thread.Sleep(5000);
        foreach (KeyValuePair<string, PeerToPeerConnection> entry in sockets)
        {
            
            Debug.Log("Manager of " + myClientName+ ": sending hello message to " + entry.Key);
            
            entry.Value.CreateAndSendMessage(helloMessage);

        }
        //annahme jetzt kennen mich einfach alle #fame
        //seclect one randomly and ask for world model
        List<string> keyList = new List<string>(sockets.Keys);

        System.Random rand = new System.Random();
        string randomKey = keyList[rand.Next(keyList.Count)];

        Debug.Log("sending get state message to " + randomKey);
        sockets[randomKey].SendGetState();
        //ask everybody for position

        /*OPTIONAL START!(gilt nicht für erste instanz)

       *
       *3.sendden einer hello message mit eigenen spielernamen(nee erst wenn alle verbindungen aufgebaut sind?)
         *4.senden eines getip -> getPeers an obigen server -> liefert ips UND PORTS!
       *5.beim erhalt der ips über einen peerToPeer client
     * a) Aufbau einer client verbindung zu jeder erhaltenen ip adresse
     * b) senden einer hello nachricht an jede der aufgebauten sockts
     * b) Spielerposition kommt als Antwort auf hello Nachricht
     *
     * OPTIONAL ENDE!
     * */
    }
    private void HandleStateMessage(StateMessage stateMessage)
    {
        Debug.Log("handling state message!");
        foreach (KeyValuePair<string, string> entry in stateMessage.playerPositions)
        {
            string nameOfPeer = entry.Key;
            Vector3 positionOfPeer = Message.ConvertStringToVector3(entry.Value);
            Debug.Log("adding player +"+nameOfPeer+"!");
            //multiplayersystem.addplayer returns MultiplayerObject which will be stored in players dict
            if (!nameOfPeer.Equals(myClientName))
            {
                Debug.Log("Manager of " + myClientName + ": i received a new player! Adding him now: "+ nameOfPeer);
                players.Add(nameOfPeer, multiplayerSystem.AddPlayer(nameOfPeer, positionOfPeer));
            }                
        }
        

        foreach (KeyValuePair<string, int> entry in stateMessage.blockPositions)
        {
            int textureID = entry.Value;
            Vector3 positionOfBlock = Message.ConvertStringToVector3(entry.Key);
            if (blockList.ContainsKey(positionOfBlock))
            {
                blockList.Remove(positionOfBlock);
            }
            blockList.Add(positionOfBlock, textureID);
            buildingSystem.PlaceBlockByPos(positionOfBlock, textureID);
        }

    }

    private void HandlePositionMessage(PositionMessage positionMessage)
    {
        //Debug.Log("handling position message. the following position has to be converted to Vector3: "+ positionMessage.position);
        Vector3 newPosition = Message.ConvertStringToVector3(positionMessage.position);
        string nameOfPeer = positionMessage.name;
        if (!players.ContainsKey(nameOfPeer))
        {
            return;
        }
        MutliplayerObject peerToMove = players[nameOfPeer];
        peerToMove.SetNewPosition(newPosition);        
    }

    private void HandleGetStateMessage(GetStateMessage getStateMessage)
    {
        // generate State Message of all playerpositions and blockpositions and send it back!


        Dictionary<string, string> playersPositions = new Dictionary<string, string>();
        playersPositions.Add(myClientName, Message.GetStringFromVector3(playersPosition));
        foreach (KeyValuePair<string, MutliplayerObject> entry in players)
        {
            string name;
            Vector3 position;
            (position, name) = entry.Value.GetPlayer();
            playersPositions.Add(name, Message.GetStringFromVector3(position));
        }


        Dictionary<string, int> blockPositions = new Dictionary<string, int>();
        foreach (KeyValuePair<Vector3, int> entry in blockList)
        {
            blockPositions.Add(Message.GetStringFromVector3(entry.Key), entry.Value);
        }

        StateMessage messageToSend = new StateMessage(playersPositions, blockPositions);
        Debug.Log("P2PConnection: Sending my State of the World with " + blockPositions.Count + " blocks and " + playersPositions.Count + " players");
        sockets[getStateMessage.senderName].CreateAndSendMessage(messageToSend);

    }




    // Update is called once per frame
    void Update()
    {
        HandleRemoveClient();

        playersPosition = player.transform.position;
        int counter = 0;
        while(incommingMessages.Count > 0 && counter < 50)
        {
            Debug.Log("Manager of "+myClientName+ ": Messages to handle this tick should not be zero: "+incommingMessages.Count);
            //TODO Statt einer Nachricht werden nun alle Nachrichten aber höchstens 10 Nachrichten abgearbeitet
            Debug.Log("Manager of "+myClientName+" received: " + incommingMessages[0].type +" MESSAGE");


            switch (incommingMessages[0].GetType().ToString())
            {

                case "PositionMessage":
                    HandlePositionMessage((PositionMessage)incommingMessages[0]);
                    counter++;
                    break;
                case "PingMessage":
                    HandlePingMessage((PingMessage)incommingMessages[0]);
                    DisplayPeersAndPings();
                    counter++;
                    break;
                case "DeleteBlockMessage":
                    HandleDeleteBlock((DeleteBlockMessage)incommingMessages[0]);
                    counter++;
                    break;
                case "AddBlockMessage":
                    HandleAddBlock((AddBlockMessage)incommingMessages[0]);
                    counter++;
                    break;
                case "StateMessage":
                    HandleStateMessage((StateMessage)incommingMessages[0]);
                    counter++;
                    break;                
                case "PeersMessage":
                    HandlePeersMessage((PeersMessage)incommingMessages[0]);
                    counter++;
                    break;
              
                case "HelloMessage":
                    HandleHelloMessage((HelloMessage)incommingMessages[0]);
                    counter++;
                    break;
                case "GetStateMessage":
                    HandleGetStateMessage((GetStateMessage)incommingMessages[0]);
                    counter++;
                    break;
                default:
                    Debug.LogError("FAILURE: Message can not be read: " + incommingMessages[0].ToString());
                    break;
            }
            //Delete Message
            incommingMessages.Remove(incommingMessages[0]);
        }        
    }
    public void AddHelloMesssage(HelloMessage helloMessage, PeerToPeerConnection connection)
    {        
        Debug.Log("Manager of " + myClientName + ": Received Hello message from: "+helloMessage.name);
        sockets.Add(helloMessage.name, connection);
        peers.Add(helloMessage.name, helloMessage.ipAndPort);
        AddIncommingMessage(helloMessage);
    }

    private void HandleHelloMessage(HelloMessage helloMessage)
    {
        players.Add(helloMessage.name, multiplayerSystem.AddPlayer(helloMessage.name, Message.ConvertStringToVector3(helloMessage.position)));
    }

    public Dictionary<Vector3, int> GetBlockPositions()
    {
        return blockList;
    }

    public Dictionary<string, MutliplayerObject> GetPlayers()
    {
        return players;
    }

    public void AddIncommingMessage(Message message)
    {
        incommingMessages.Add(message);
    }

    
    public string GetLocalIPAddress()
    {
        if (localIpAdress.Equals(""))
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
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
}
