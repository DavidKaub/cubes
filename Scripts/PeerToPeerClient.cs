using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Net;


public class PeerToPeerClient : PeerToPeerConnection
{

    private string ip;
    public bool isConnected = false;


    public PeerToPeerClient(PeerToPeerManager manager, string ip, int port, string clientName) : base(manager)
    {
        this.ip = ip;
        this.port = port;
        this.clientName = clientName;
        ConnectToTcpServer();
    }

       
    private void ConnectToTcpServer()
    {
        SSTools.ShowMessage("Connecting to server", SSTools.Position.bottom, SSTools.Time.twoSecond);
        try
        {
            Debug.Log("Client: Connecting to Server!");
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
            Debug.Log("Client: Client Thread to Server started!");
            isConnected = true;
        }
        catch (Exception e)
        {
            Debug.Log("Client: On client connect exception " + e);
        }
    }

    /// <summary> 	
    /// Runs in background clientReceiveThread; Listens for incomming data. 	
    /// </summary>     
    private void ListenForData()
    {
        try
        {
            socketConnection = new TcpClient(ip, port);
            socketConnection.SendTimeout= 2000;
            socketConnection.Client.SetSocketOption(SocketOptionLevel.Socket ,SocketOptionName.NoDelay, 1);
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                Debug.Log("Client: Waiting for new Message from Server!");
                // Get a stream object for reading 				
                //socketConnection.Connect(ip, port);
                NetworkStream stream = socketConnection.GetStream();
                
                    int length;
                    // Read incomming stream into byte arrary. 					
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        Debug.Log("Client: received new Message from Server!");
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message. 						
                        string serverMessage = Encoding.UTF8.GetString(incommingData);
                        Debug.Log("Client: #####################\nserver message received: " + serverMessage);
                        HandleIncommingMessage(serverMessage);
                    }
                
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Client: Socket exception with socket ip= "+ip + " and port= " + port +" with message: " + socketException);
            socketConnection.Close();
            peerToPeerManager.RemoveClient(clientName);

        }
        Debug.Log("Client: Connection closed?!?");

    }



  


    override public void SendToPeer(string message)
    {
        if (socketConnection == null)
        {
            Debug.LogWarning("Client: no socket defined!");
            return;
        }
        try
        {
            Debug.Log("Client: Sending Message: " + message + " to Server via Socket!");
            // Get a stream object for writing.
            Debug.Log("trying to get stream");
            try{

            socketConnection.Connect(IPAddress.Parse(ip), port);
            } catch (SocketException ex){
                Debug.Log("Client: I think the socket can not connect: " + ex);
            }
            NetworkStream stream = socketConnection.GetStream();
            Debug.Log("got stream");
            if (stream.CanWrite)
            {
                Debug.Log("can write!");
                // Convert string message to byte array.  
                message += "\n";
                byte[] clientMessageAsByteArray = Encoding.UTF8.GetBytes(message);
                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                Debug.Log("Client: Client sent his message - should be received by server");
                //stream.Close();
                //stream.EndWrite(asyncResult);
                stream.Flush();
            }
            else
            {
                Debug.Log("PeerToPeerClient cannot write!");
            }
        }
        
        catch (InvalidOperationException e)
        {
            Debug.Log("Client: Invalid Operation Exception: " + e);
            socketConnection.Close();
            peerToPeerManager.RemoveClient(clientName);

        }
        catch (SocketException e) {
            Debug.Log("Client: Socket Exception: " + e);
            socketConnection.Close();
            peerToPeerManager.RemoveClient(clientName);


        }
        catch (Exception e) {
            Debug.Log("Client: Exception: " + e);
            socketConnection.Close();
            peerToPeerManager.RemoveClient(clientName);
        }
        

    }

   
}
