// CallasOscReceiver.cs
// author: Johannes Wagner <johannes.wagner@informatik.uni-augsburg.de>
// created: 2009/02/04
// Copyright (C) University of Augsburg

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Ventuz.OSC;

namespace CallasOsc
{
    public delegate void OscListenerHandler (OscElement element);

    class Listener
    {
        internal void receive(OscElement element)
        {
            switch (element.Address)
            {
                case "/text":
                    recv_message((String)element.Args[0], (float)element.Args[1], (String)element.Args[2]);
                    break;
                case "/evnt":
                    int num = (int) element.Args[3];
                    String[] names = new String[num];
                    float[] values = new float[num];
                    int j = 4;
                    for (int i = 0; i < num; i++)
                    {
                        names[i] = (String)element.Args[j++];
                        values[i] = (float)element.Args[j++];
                    }
                    recv_event((String)element.Args[0], (float)element.Args[1], (float)element.Args[2], num, names, values);
                    break;
                case "/strm":
                    recv_stream((String)element.Args[0], (float)element.Args[1], (float)element.Args[2], (int) element.Args[3], (byte[]) element.Args[4]);
                    break;
            }
        }

        public virtual void recv_message(String id, float time, String msg)
        {
        }

        public virtual void recv_event(String id, float time, float dur, int num, String[] event_names, float[] event_values)
        {
        }

        public virtual void recv_stream(String id, float time, float sr, int dim, byte[] data)
        {
        }
    }

    class Receiver
    {
   
        int port;
        UdpClient udp_client;
        IPEndPoint ip;
        bool is_running = true;

        public event OscListenerHandler listener_event;

        public Receiver(int port, OscListenerHandler listener)
        {
            listener_event += listener;
            this.port = port;
            ip = new IPEndPoint(IPAddress.Any, port);
            is_running = false;
            
        }

        public void start()
        {
            udp_client = new UdpClient(port);                        
            is_running = true;
            WaitForData();   
        
        }

        public void stop()
        {
            is_running = false;
            udp_client.Close();
        }
       
        private void WaitForData()
        {       
            try
            {
                udp_client.BeginReceive(new AsyncCallback(OnDataReceived), true);
            }
            catch (ObjectDisposedException e)
            {
                if (is_running)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void OnDataReceived(IAsyncResult asyn)
        {
            if (is_running == true)
            {
                byte[] bytes = udp_client.EndReceive(asyn, ref ip);

                byte[] length = BitConverter.GetBytes(bytes.Length);

                byte[] bytes2 = new byte[bytes.Length + length.Length];
                for (int i = 0; i < length.Length; i++)
                {
                    bytes2[i] = length[i];
                }
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes2[i + length.Length] = bytes[i];
                }
                Stream s = new MemoryStream(bytes2);
                OscElement element = OscElement.FromStream(s);

                listener_event(element);
                             
                WaitForData();
            }
            else { }
        }
    }
}
