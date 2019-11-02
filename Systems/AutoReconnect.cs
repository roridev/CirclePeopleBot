using System.Net.Sockets;
using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Lolibase.Discord.Utils;

namespace Lolibase.Discord.Systems
{
    public class AutoReconnect : IApplyToClient, IApplicableSystem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }

        public void Activate()
        {
            Active = true;
            Name = "AutoReconnect";
            Description = "Automatically reconnects the bot in case of a socket closing / erroring.";
        }

        public void ApplyToClient(DiscordClient client)
        {
            client.SocketClosed += SocketClosed;
            client.SocketOpened += SocketOpened;
            client.SocketErrored += SocketErrored;
        }

        private async Task SocketOpened()
        {
            Console.WriteLine("[Reconnect] Conection to Websocket stablished.");
        }

        private async Task SocketErrored(SocketErrorEventArgs e)
        {
            Console.WriteLine("[Reconnect] Websocket Errored. Attempting to reconnect.");
            await e.Client.DisconnectAsync();
            await e.Client.ConnectAsync();
        }

        private async Task SocketClosed(SocketCloseEventArgs e)
        {
            Console.WriteLine("[Reconnect] Websocket Closed. Attempting to reconnect.");
            await e.Client.DisconnectAsync();
            await e.Client.ConnectAsync();
        }

        public void Deactivate()
        {
            throw new System.NotImplementedException();
        }
    }
}