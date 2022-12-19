using System;
using System.Linq;
using MyJetWallet.B2C2.Client.Models.Rest;

namespace MyJetWallet.B2C2.Client.Exceptions
{
    // ReSharper disable once InconsistentNaming
    public class B2c2RestException : Exception
    {
        public ErrorResponse ErrorResponse { get; }

        public Guid RequestId { get; }

        public B2c2RestException(ErrorResponse errorResponse, Guid requestId)
        {
            ErrorResponse = errorResponse;
            RequestId = requestId;
        }

        public B2c2RestException(string message, Exception e, Guid requestId) : base(message, e)
        {
            RequestId = requestId;
        }

        public B2c2RestException(string message, Guid requestId) : base(message)
        {
            RequestId = requestId;
        }

        public override string Message
        {
            get
            {
                if (ErrorResponse != null)
                    return $"{ErrorResponse.Errors.FirstOrDefault()?.Code} : {ErrorResponse.Errors.FirstOrDefault()?.Message}, guid: {RequestId}";

                return $"Message: '{base.Message}', guid: {RequestId}";
            }
        }
    }
}
