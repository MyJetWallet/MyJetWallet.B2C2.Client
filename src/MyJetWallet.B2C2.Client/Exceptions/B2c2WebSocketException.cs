using System;
using MyJetWallet.B2C2.Client.Models.WebSocket;

namespace MyJetWallet.B2C2.Client.Exceptions
{
    public class B2c2WebSocketException : Exception
    {
        public ErrorResponse ErrorResponse { get; }

        public B2c2WebSocketException(string message, Exception e) : base(message, e)
        {
        }

        public B2c2WebSocketException(string message) : base(message)
        {
        }

        public B2c2WebSocketException(string message, ErrorResponse errorResponse) : base(message)
        {
            ErrorResponse = errorResponse;
        }
    }
}
