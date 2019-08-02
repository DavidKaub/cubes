using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/*
 * This class handles an active connection i.e. a socket connection to another peer (as a server)
 * 
 */

public class PeerToPeerClientConnect : PeerToPeerConnection
{
    // Start is called before the first frame update

    public PeerToPeerClientConnect(TcpClient connectedTcpClient, PeerToPeerManager managerInstance):base(managerInstance)
    {
        this.socketConnection = connectedTcpClient;
        socketConnection.SendTimeout = 2000;
        Debug.Log("Client conect created!");
        
        clientReceiveThread = new Thread(new ThreadStart(HandleInput));
        clientReceiveThread.IsBackground = true;
        clientReceiveThread.Start();

        //HandleInput();
        
    }





    private void HandleInput()
    {
        Debug.Log("Server Client connect handling input!");
        Byte[] bytes = new Byte[1024];
        while (true)
        {
            try
            {

                // Get a stream object for reading 					
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    int length;
                    // Read incomming stream into byte arrary. 						
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message. 							
                        string clientMessage = Encoding.ASCII.GetString(incommingData);
                        Debug.Log("client message received by server socket as: " + clientMessage);

                        HandleIncommingMessage(clientMessage);
                    }
                }

            }
            catch (SocketException socketException)
            {
                socketConnection.Close();
                peerToPeerManager.RemoveClient(clientName);
                Debug.Log("Socket exception: " + socketException);
            }
            catch(Exception e)
            {
                Debug.Log("other exeption: "+ e);
                socketConnection.Close();
                peerToPeerManager.RemoveClient(clientName);
            }

           
        }
    }

 override public void SendToPeer(string message)
{
        if (socketConnection == null)
        {
            return;
        }

        try
        {
            // Get a stream object for writing. 	
            Debug.Log("peer to peer client connect sending message " + message);
            NetworkStream stream = socketConnection.GetStream();
            Debug.Log("got stream!");
            if (stream.CanWrite)
            {
                message += "\n";
                // Convert string message to byte array.                 
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(message);
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                Debug.Log("Server sent his message - should be received by client");
                stream.Flush();
            }
        }
        catch (SocketException socketException)
        {
            socketConnection.Close();
            peerToPeerManager.RemoveClient(clientName);
            Debug.Log("Socket exception: " + socketException);
        }
    }
}
