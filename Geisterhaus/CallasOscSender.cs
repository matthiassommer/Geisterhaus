// CallasOscSender.cs
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
    class Sender
    {
        Socket s;
        IPEndPoint ep;
        IPAddress ip;
        int port;

        public Sender(IPAddress ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public void send_message(String id, float time, String text)
        {
            Object[] o = new Object[3];
            o[0] = id;
            o[1] = time;
            o[2] = text;

            send("/text", o);
        }

        public void send_event(String id, float time, float dur, int size, String[] names, float[] values)
        {
            Object[] o = new Object[4 + size * 2];
            o[0] = id;
            o[1] = time;
            o[2] = dur;
            o[3] = size;
            for (int i = 0; i < size; i++)
            {
                o[4 + 2 * i] = names[i];
                o[4 + 2 * i + 1] = values[i];
            }

            send("/evnt", o);
        }

        public void send_stream(String id, float time, float sample_rate, int dimension, byte[] data)
        {
            Object[] o = new Object[5];
            o[0] = id;
            o[1] = time;
            o[2] = sample_rate;
            o[3] = dimension;
            o[4] = data;

            send("/strm", o);
        }

        public void send_stream(String id, float time, float sample_rate, int dimension, float[] data)
        {
            Object[] o = new Object[5];
            o[0] = id;
            o[1] = time;
            o[2] = sample_rate;
            o[3] = dimension;
            byte[] data_as_bytes = new byte[data.Length * sizeof(float)];
            FloatToByte(data_as_bytes, data);
            o[4] = data_as_bytes;

            send("/strm", o);
        }

        static private void FloatToByte(byte[] output, float[] input)
        {
            int x = 0;
            foreach (float f in input)
            {
                byte[] t = BitConverter.GetBytes(f);
                for (int y = 0; y < sizeof (float); y++)
                {
                    output[y + x] = t[y];
                }
                x += sizeof(float);
            }
        }

        public void send_stream(String id, float time, float sample_rate, int dimension, double[] data)
        {
            Object[] o = new Object[5];
            o[0] = id;
            o[1] = time;
            o[2] = sample_rate;
            o[3] = dimension;
            byte[] data_as_bytes = new byte[data.Length * sizeof(double)];
            DoubleToByte(data_as_bytes, data);
            o[4] = data_as_bytes;

            send("/strm", o);
        }

        static private void DoubleToByte(byte[] output, double[] input)
        {
            int x = 0;
            foreach (double d in input)
            {
                byte[] t = BitConverter.GetBytes(d);
                for (int y = 0; y < sizeof (double); y++)
                {
                    output[y + x] = t[y];
                }
                x += sizeof(double);
            }
        }

        private void send(String tag, Object[] parameter)
        {
            OscElement OscElement = new OscElement(tag, parameter);
            Stream stream = new MemoryStream();
            OscElement.ToStream(stream);
            byte[] output = new byte[stream.Length];
            stream.Position = 0;
            ByteToStream(stream, output);

            byte[] output2 = new byte[output.Length - 4];
            for (int i = 0; i < output2.Length; i++)
            {
                output2[i] = output[i + 4];
            }
            s.SendTo(output2, ep);
        }

        public void start()
        {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ep = new IPEndPoint(ip, port);
        }

        public void stop()
        {
            s.Shutdown(SocketShutdown.Both);
        }

        static public void ByteToStream(Stream output, byte[] input)
        {
            int offset = 0;
            int count = input.Length;
            output.Read(input, offset, count);
        }
    }
}
