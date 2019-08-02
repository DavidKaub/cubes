using System;
using System.Collections.Generic;
using UnityEngine;


abstract public class Message
{
    public string type;


    public static string GetStringFromVector3(Vector3 vector)
    {
        string toReturn = "(" + vector.x + "," + vector.y + "," + vector.z + ")";
        //Debug.Log("STRINGtoVEC: " + toReturn);
        return toReturn;
    }


    public static Vector3 ConvertStringToVector3(string input)
    {
        input = input.Replace("(","");
        input = input.Replace(")","");
        //Debug.Log("String to vector method with input: "+input);
        string[] tokens = input.Split(',');
        //int[] coordinates = Array.ConvertAll<string, int>(tokens, int.Parse);
        return new Vector3(float.Parse(tokens[0]), float.Parse(tokens[1]), float.Parse(tokens[2]));
    }
}

public class PingMessage : Message
{
    public long send;
    public long received;
    public long ping;
    public bool isResponse;
    public string receiver;

    public PingMessage(string receiver)
    {
        type = "ping";
        send = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        isResponse = false;
        this.receiver = receiver;
    }
    public void Received()
    {
        received = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        ping = (received - send);
    }
    public void Respond()
    {
        isResponse = true;
    }
}


public class AddBlockMessage : Message
{
    public string position;
    public int texture;

    public AddBlockMessage()
    {
        type = "addBlock";
    }

    public AddBlockMessage(Vector3 position, int texture)
    {
        type = "addBlock";
        this.position = Message.GetStringFromVector3(position);
        this.texture = texture;
    }
}


public class DeleteBlockMessage : Message
{
    public string position;
    public DeleteBlockMessage()
    {
        type = "deleteBlock";
    }

    public DeleteBlockMessage(Vector3 position)
    {
        type = "deleteBlock";
        this.position = Message.GetStringFromVector3(position);
    }
}


public class ChatMessage : Message
{
    public string sender;
    public string receiver;
    public string content;
    public bool world;
    public string timeReceivedAt;

    public ChatMessage()
    {
        timeReceivedAt = string.Format("{0:HH:mm:ss}", DateTime.Now);
    }

    public ChatMessage(string sender, string content, bool world)
    {
        type = "chat";
        this.sender = sender;
        this.content = content;
        this.world = world;
        timeReceivedAt = string.Format("{0:HH:mm:ss}", DateTime.Now);
    }

    public override string ToString()
    {
        if (world)
        {
            return "(" + timeReceivedAt + ") WorldMessage from " + sender + ": " + content;
        }
        return "(" + timeReceivedAt + ") " + sender + " to " + receiver + ": \"" + content + "\"";
    }
}

public class HelloMessage : Message
{
    public string name;
    public string position;
    public string ipAndPort;

    public HelloMessage()
    {
        type = "hello";
    }

    public HelloMessage(string name, Vector3 position, string ipAndPort)
    {
        type = "hello";
        this.name = name;
        this.position = GetStringFromVector3(position);
        this.ipAndPort = ipAndPort;
    }
}



public class GetPeersMessage : Message
{
    public GetPeersMessage()
    {
        type = "getPeers";
    }
}



public class PeersMessage : Message
{

    //<name,ip:port>
    public Dictionary<string, string> peers;

    public PeersMessage()
    {
        type = "peers";
    }

    public PeersMessage(Dictionary<string, string> peers)
    {
        type = "peers";
        this.peers = peers;
    }

}

public class PositionMessage : Message
{
    public string position;
    public string name;

    public PositionMessage()
    {
        type = "position";
    }
    public PositionMessage(Vector3 position, string name)
    {
        type = "position";
        this.position = Message.GetStringFromVector3(position);
        this.name = name;
    }
}



public class StateMessage : Message
{
    //Map<String,String> aller bekannten Clients (Key=Name des Clients, Value = dessen Position): {"type":"state","positions":{"client1":"position1","client2":"position2","usw":"usf"}}
    //Position[] positions;

    //<name,position>
    public Dictionary<string, string> playerPositions;
    //<position,texture>
    public Dictionary<string, int> blockPositions;

    public StateMessage()
    {
        type = "state";
    }

    public StateMessage(Dictionary<string, string> playerPositions, Dictionary<string, int> blockPositions)
    {
        type = "state";
        this.blockPositions = blockPositions;
        this.playerPositions = playerPositions;
    }


  


    public override string ToString()
    {
        string returnString = "State Message: ";
        if (blockPositions == null && playerPositions == null)
        {
            returnString += " is null";
            return returnString;
        }
        if(playerPositions != null)
        {
            foreach (KeyValuePair<string, string> entry in playerPositions)
            {
                returnString += "\n " + entry.Key + " = " + entry.Value;
            }
        }
        if(blockPositions != null)
        {
            foreach (KeyValuePair<string, int> entry in blockPositions)
            {
                returnString += "\n " + entry.Key + " = " + entry.Value;
            }
        }
        return returnString;
    }
}

public class GetStateMessage : Message
{

    public string senderName;

    public GetStateMessage()
    {
        type = "getState";
    }

    public GetStateMessage(string senderName)
    {
        type = "getState";
        this.senderName = senderName;
    }
}


