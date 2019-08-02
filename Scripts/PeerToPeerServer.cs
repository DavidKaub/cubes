using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


/*
 * This class should only be initialized once -> waits for incomming connections
 * 
 */


public class PeerToPeerServer
{
    int port;
    PeerToPeerManager managerInstance;
    TcpListener tcpListener;
    TcpClient tcpClient;
    private Thread tcpListenerThread;


    public PeerToPeerServer(PeerToPeerManager managerInstance, int port)
    {
        this.managerInstance = managerInstance;
        this.port = port;
    }

    //when accepting a new connection a PeerToPeerClientConnect instance is cerated an deligated to the managerInstance


   
    

    // Start is called before the first frame update
    public void Start()
    {
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }


    private void ListenForIncommingRequests()
    {
        try
        {
            // Create listener on localhost port 8052. 	
            
            Debug.Log("port = " + port);
            tcpListener = new TcpListener(IPAddress.Parse("0.0.0.0"), port);

            
            //Socket listenerSocket = tcpListener.Server;		
            //LingerOption lingerOption = new LingerOption(true, 10);
            //listenerSocket.SetSocketOption(SocketOptionLevel.Socket, 
            //          SocketOptionName.Linger, 
            //          lingerOption);

                      
            //tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);


            Debug.Log("Server: established server on port "+port);
            //tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            tcpListener.Start();
            Debug.Log("Server: Server is listening on port "+port);
            //Byte[] bytes = new Byte[1024];
            while (true)
            {
                Debug.Log("Waiting for new Client!");
                TcpClient client = tcpListener.AcceptTcpClient();
                Debug.Log("Server: Client connected to server!!");
                PeerToPeerClientConnect clientConnect = new PeerToPeerClientConnect(client, managerInstance);
                Debug.Log("new Client connected!");

            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Server: SocketException " + socketException.ToString());
        }
    }


    public void OnApplicationQuit()
    {
        try
        {
            tcpListener.Stop();
            tcpListenerThread.Abort();
        } catch (Exception e)
        {
            Debug.Log("Server: Cant stop tcpListener!!");
            Debug.Log(e.ToString());
        }
    }

}
