using System.Net.Sockets;
using System.Net;

namespace VegaExpress.Worker.Extentions
{
    public static class TcpListenerExtention
    {
        public static bool ValidePort(this TcpListener listener, string ip, int port)
        {
            try
            {
                // Intenta abrir un nuevo TcpListener
                listener = new TcpListener(IPAddress.Parse(ip), port);
                listener.Start();

                // No olvides detener el listener cuando hayas terminado
                listener.Stop();
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    // El puerto ya está en uso
                    return false;
                }
                else
                {
                    // Si es otro tipo de SocketException, vuelve a lanzarla
                    throw;
                }
            }

            // El puerto no está en uso
            return true;
        }
    }
}
