using System.ServiceModel;
using System.Security.Cryptography;

namespace Chatick
{
    [ServiceContract]
    public interface IP2PService
    {
        [OperationContract]
        string GetName();

        [OperationContract]
        RSAParameters GetPublicKey();

        [OperationContract(IsOneWay = true)]
        void SendMessage(byte[] message, string from);
    }
}
