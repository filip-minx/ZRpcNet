using NetMQ;

namespace Minx.ZRpcNet.Encryption
{
    public static class EncryptionExtensions
    {
        public static ZRpcClient UseEncryption(this ZRpcClient client, byte[] serverPublicKey)
        {
            client.Options.CurveServerPublicKey = serverPublicKey;

            client.subscriberSocket.Options.CurveServerKey = serverPublicKey;
            client.subscriberSocket.Options.CurveCertificate = new NetMQCertificate();

            return client;
        }

        public static ZRpcServer UseEncryption(this ZRpcServer server, NetMQCertificate certificate)
        {
            server.responseSocket.Options.CurveServer = true;
            server.responseSocket.Options.CurveCertificate = certificate;

            server.publisherSocket.Options.CurveServer = true;
            server.publisherSocket.Options.CurveCertificate = certificate;

            return server;
        }
    }
}
