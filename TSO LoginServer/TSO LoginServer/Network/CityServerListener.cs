﻿/*The contents of this file are subject to the Mozilla Public License Version 1.1
(the "License"); you may not use this file except in compliance with the
License. You may obtain a copy of the License at http://www.mozilla.org/MPL/

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

The Original Code is the TSO LoginServer.

The Initial Developer of the Original Code is
Mats 'Afr0' Vederhus. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TSO_LoginServer.Network
{
    public delegate void OnCityReceiveDelegate(PacketStream P, CityServerClient Client);

    public class CityServerListener
    {
        private List<CityServerClient> m_CityServers;
        private Socket m_ListenerSock;
        private IPEndPoint m_LocalEP;

        /// <summary>
        /// The CityServers that are currently connected to this LoginServer.
        /// </summary>
        public List<CityServerClient> CityServers
        {
            get { return m_CityServers; }
        }

        public event OnCityReceiveDelegate OnReceiveEvent;

        public CityServerListener()
        {
            m_ListenerSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_CityServers = new List<CityServerClient>();
        }

        public void Initialize(int Port)
        {
            IPEndPoint LocalEP = new IPEndPoint(IPAddress.Any, Port);

            m_LocalEP = LocalEP;

            try
            {
                m_ListenerSock.Bind(LocalEP);
                m_ListenerSock.Listen(10000);

                Console.WriteLine("Started listening on: " + LocalEP.Address.ToString()
                    + ":" + LocalEP.Port);
            }
            catch (SocketException E)
            {
                Console.WriteLine("Winsock error caused by call to Socket.Bind(): \n" + E.ToString());
            }

            m_ListenerSock.BeginAccept(new AsyncCallback(OnAccept), m_ListenerSock);
        }

        public void OnAccept(IAsyncResult AR)
        {
            Socket AcceptedSocket = m_ListenerSock.EndAccept(AR);

            if (AcceptedSocket != null)
            {
                Console.WriteLine("\nNew client connected!");

                //Let sockets linger for 5 seconds after they're closed, in an attempt to make sure all
                //pending data is sent!
                AcceptedSocket.LingerState = new LingerOption(true, 5);
                m_CityServers.Add(new CityServerClient(AcceptedSocket, this));
            }

            m_ListenerSock.BeginAccept(new AsyncCallback(OnAccept), m_ListenerSock);
        }

        /// <summary>
        /// Called by PatchClient instances
        /// when they've received some new data
        /// (a new packet). Should not be called
        /// from anywhere else.
        /// </summary>
        /// <param name="P"></param>
        public void OnReceivedData(PacketStream P, CityServerClient Client)
        {
            OnReceiveEvent(P, Client);
        }
    }
}
