using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Tophat_Response_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            return;

            using (SslStream sslStream = Init_ServerCommunication("arboroia.com", 443))
            {
                byte[] buffer = new byte[4096];
                int index = 0;
                int x;

                while ((x = sslStream.ReadByte()) != -1)
                {
                    buffer[index] = (byte)x;
                }

                Console.WriteLine(Encoding.UTF8.GetString(buffer));

                Console.WriteLine("Finished. Press any key to close the connection...");
                Console.ReadKey();
            }
        }


        static bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public struct TopHatHeader
        {
            byte _ver;
            byte _opcode;
            ushort _response;
        }

        public static SslStream Init_ServerCommunication(string URL, int Port)
        {
            UTF8Encoding encoder = new UTF8Encoding();

            //
            //First create the header
            //
            byte ver = 2;
            byte opcode = 0;
            ushort response = 0;
            string uri = @"h";
            string data = "Hiya: \"hi\"";

            byte[] header = new byte[8];
            header[0] = ver;
            header[1] = opcode;
            header[2] = BitConverter.GetBytes(response)[0];
            header[3] = BitConverter.GetBytes(response)[1];

            if (encoder.GetByteCount(uri) > ushort.MaxValue)
                throw new Exception("The URI is too large to send");
            ushort uriLength = (ushort)encoder.GetByteCount(uri);

            if (encoder.GetByteCount(data) > ushort.MaxValue)
                throw new Exception("The data is too large to send");
            ushort dataLength = (ushort)encoder.GetByteCount(data);

            header[4] = BitConverter.GetBytes(uriLength)[0];
            header[5] = BitConverter.GetBytes(uriLength)[1];
            header[6] = BitConverter.GetBytes(dataLength)[0];
            header[7] = BitConverter.GetBytes(dataLength)[1];

            TcpClient clientSocket = new TcpClient(URL, Port);

            SslStream sslStream = new SslStream(clientSocket.GetStream(), false, new RemoteCertificateValidationCallback(CertificateValidationCallback));

            sslStream.AuthenticateAsClient(URL);

            sslStream.Write(header, 0, 8);
            sslStream.Write(encoder.GetBytes(uri), 0, uriLength);
            sslStream.Write(encoder.GetBytes(data), 0, dataLength);

            return sslStream;
        }
    }
}
