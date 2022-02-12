using LiveSplit.UI.Components;
using System;
using LiveSplit.Model;
using LiveSplit.UI;
using System.Xml;
using System.Threading;
using System.IO;
using System.Net.Sockets;

namespace LiveSplit.NotesCreatorAPI
{
    public class NotesAPIcomponent : LogicComponent
    {
        private LiveSplitState m_state = null;

        private static Thread API_ConnectionThread = new Thread(API_Connection_class.Connect) { Name = "API_ConnectionThread" };
        private static Thread API_ListenerThread;

				private static System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        private static System.Net.Sockets.NetworkStream serverStream;
        private static bool tryToConnect = true;
        private static bool listenToServer = false;
        private static bool sendActions = false;

        private class API_Connection_class
				{
            public static void Connect()
						{
                tryToConnect = true;
                while (tryToConnect)
                    try
                    {
                        clientSocket.Connect("localhost", 8888);
                        System.Diagnostics.Debug.WriteLine("Connection established at 127.0.0.1:8888");
                        tryToConnect = false;
                        serverStream = clientSocket.GetStream();
                        API_ListenerThread?.Abort();
                        API_ListenerThread = new Thread(API_Listener_class.Listen)
                        {
                            Name = "API_ListenerThread"
                        };
                        API_ListenerThread.Start();
                    } catch (SocketException) {
                        System.Diagnostics.Debug.WriteLine("SocketException while connecting, maybe there is no server at the specidied IP! Tring again!");
                    } catch (Exception) {
                        System.Diagnostics.Debug.WriteLine("Error while connecting! Tring again!");
                    }
                System.Diagnostics.Debug.WriteLine("Exiting API_ConnectionThread!");
            }
				}

        private class API_Listener_class
				{
            public static void Listen()
						{
                listenToServer = true;
                StreamReader reader = new StreamReader(serverStream);
                while (listenToServer)
								{
                    ParseCommunication(reader.ReadLine() );
                }
                System.Diagnostics.Debug.WriteLine("Exiting API_ListenerThread!");
            }
				}
        
        private static void ParseCommunication(string message)
        {
            switch (message)
            {
                case "holdActionCommunication":
                    sendActions = false;
                    break;
                case "startActionCommunication":
                    sendActions = true;
                    break;
                case "closeConnection":
                    System.Diagnostics.Debug.WriteLine("Input: " + message + "; now closing connection to server");

                    listenToServer = false;
                    System.Diagnostics.Debug.WriteLine("No more inputs from server are read");

                    SendToAPI("closedConnection");
                    clientSocket.Dispose();
                    clientSocket = new System.Net.Sockets.TcpClient(); 
                    System.Diagnostics.Debug.WriteLine("Output: closedConnection");

                    API_ConnectionThread = new Thread(API_Connection_class.Connect)
                    {
                        Name = "API_ConnectionThread"
                    };
                    System.Timers.Timer timer = new System.Timers.Timer();
                    timer.Elapsed += ((e, o) => { System.Diagnostics.Debug.WriteLine("Startig Connection Thread"); API_ConnectionThread.Start(); });
                    timer.Interval = 2000;
                    timer.AutoReset = false;
                    timer.Start();
                    System.Diagnostics.Debug.WriteLine("API_ConnectionThread restarting in 2 seconds: trying to connect to next server");

                    break;
                case "closedConnection":
                    System.Diagnostics.Debug.WriteLine("Input: " + message + "; connection closed by server, now aborting all connection threads!");
                    API_ListenerThread.Abort();
                    System.Diagnostics.Debug.WriteLine("Both API_ListenerThread and API_ConnectionThread aborted!");
                    break;
            }
        }

        private static void SendToAPI(string str)
        {
            try
            {
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(str + "\n");
                serverStream.Write(outStream, 0, outStream.Length);
            }
            catch (Exception)
            {
                if (!API_ConnectionThread.IsAlive)
                {
                    API_ConnectionThread.Abort();
										API_ConnectionThread = new Thread(API_Connection_class.Connect)
										{
												Name = "API_ConnectionThread"
										};
										API_ConnectionThread.Start();
                }
            }
        }

        private static void SendActionToAPI(string message)
        {
            if (sendActions)
                SendToAPI(message);
        }

        private void OnStart(object sender, EventArgs e)
				{
            SendActionToAPI("start");
				}

        public void OnSplit(object sender, EventArgs e)
				{
            SendActionToAPI("split");
        }

        public void OnUndoSplit(object sender, EventArgs e)
        {
            SendActionToAPI("undoSplit");
        }

        public void OnSkipSplit(object sender, EventArgs e)
        {
            SendActionToAPI("skipSplit");
        }

        public void OnReset(object sender, TimerPhase value)
        {
            SendActionToAPI("reset");
        }

				#region LogicComponent Implementation
				public override string ComponentName => NotesAPIFactory.s_componentName;

				public NotesAPIcomponent(LiveSplitState state)
				{
            API_ConnectionThread.Start();
            m_state = state;
            m_state.OnStart += OnStart;
            m_state.OnSplit += OnSplit;
            m_state.OnUndoSplit += OnUndoSplit;
            m_state.OnSkipSplit += OnSkipSplit;
            m_state.OnReset += OnReset;
				}


				public override void Dispose()
        {
            if (clientSocket.Connected)
						    SendToAPI("closeConnection");
            tryToConnect = false;
            m_state.OnStart -= OnStart;
            m_state.OnSplit -= OnSplit;
            m_state.OnUndoSplit -= OnUndoSplit;
            m_state.OnSkipSplit -= OnSkipSplit;
            m_state.OnReset -= OnReset;
        }

        public override XmlNode GetSettings(XmlDocument document)
        {
            return document.CreateElement("Settings");
        }

        public override System.Windows.Forms.Control GetSettingsControl(LayoutMode mode)
        {
            return null;
        }

        public override void SetSettings(XmlNode settings)
        {
            
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            
        }

				#endregion
		}
}
