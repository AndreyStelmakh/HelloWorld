using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BlueButton
{
    internal static class Device
    {
        static System.Net.Sockets.Socket _socket;
        public static void Execute(IPEndPoint IPEndPoint, byte[] command)
        {
            _socket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Tcp);
            var arg = new System.Net.Sockets.SocketAsyncEventArgs() { RemoteEndPoint = IPEndPoint };

            arg.SetBuffer(command, 0, command.Length);
            arg.Completed += Sending_Completed;

            var result = _socket.ConnectAsync(arg);


            //socket.SendToAsync(arg);

        }

        private static void Sending_Completed(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            var arg = new System.Net.Sockets.SocketAsyncEventArgs() {   };
            arg.Completed += Receiving_Completed;
            arg.SetBuffer(new byte[1000], 0, 1000);
            _socket.ReceiveAsync(arg);

            //throw new NotImplementedException();

        }

        private static void Receiving_Completed(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            _socket.Dispose();
            _socket = null;

        }

        public static IEnumerable<byte> ToUtf8Bytes(this string src)
        {
            var result = new byte[0].AsEnumerable();

            foreach (var ch in src.ToArray())
            {
                result = result.Concat(Encoding.Convert(Encoding.Unicode, Encoding.UTF8, BitConverter.GetBytes(ch)));

            }

            return result;

        }

    }

    class InsCommand
    {
        public static Result Execute(IPEndPoint endPoint, Windows.UI.Xaml.Media.Imaging.BitmapImage bitmap)
        {
            string debug_value = "FF0FF0FF0FF0FF0F00F00F00F00F00FF0FF0FF0FF0FF000F00F00F00F00FF00F00F00F00F0000F00F00F00F00FF00F00F00F00F00F555F5F5555FF555F5F5555F";

            byte[] new_line = new byte[] { 0xD, 0xA };
            var cmd = (string.Format("INS ") + debug_value)
                .ToUtf8Bytes()
                .Concat(new_line)
                .ToArray();

            Device.Execute(endPoint, cmd);

            return new Result();

        }

        public class Result { }

    }

    class DispCommand
    {
        public static Result Execute(IPEndPoint endPoint, string symbolName)
        {
            byte[] r = new byte[] { 0 };
            var cmd1 = string.Format("disp {0}", symbolName)
                .ToUtf8Bytes().ToArray();
            var cmd = cmd1
                .Concat(r)
                .ToArray();

            Device.Execute(endPoint, cmd);

            return new Result();

        }

        public class Result { }

    }

    class RetrCommand
    {
    }



}
