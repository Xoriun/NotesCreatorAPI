using LiveSplit.UI.Components;
using System;
using LiveSplit.Model;
using LiveSplit.UI;
using System.Xml;
using System.Threading;

namespace LiveSplit.NotesCreatorAPI
{
    public class NotesAPIcomponent : LogicComponent
    {
        private LiveSplitState m_state = null;

        private Thread API_Thread = new Thread(API_Thread_class.Connect);

				protected static System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        protected static System.Net.Sockets.NetworkStream serverStream;

        private class API_Thread_class
				{
            public static void Connect()
						{
                while (!clientSocket.Connected)
                    try
                    {
                        clientSocket.Connect("127.0.0.1", 8888);
                        serverStream = clientSocket.GetStream();
                    }
                    catch (Exception)
                    {
                        System.Diagnostics.Debug.WriteLine("Connection failed! Tring again!");
                    }
            }
				}

        private void SendToAPI(string str)
        {
            //System.Diagnostics.Debug.WriteLine(str);

            if (clientSocket.Connected)
            {
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(str + "\n");
                serverStream.Write(outStream, 0, outStream.Length);
            }
        }

        private void OnStart(object sender, EventArgs e)
				{
            SendToAPI("start");
				}

        public void OnSplit(object sender, EventArgs e)
				{
            SendToAPI("split");
        }

        public void OnUndoSplit(object sender, EventArgs e)
        {
            SendToAPI("undoSplit");
        }

        public void OnSkipSplit(object sender, EventArgs e)
        {
            SendToAPI("skipSplit");
        }

        public void OnReset(object sender, TimerPhase value)
        {
            SendToAPI("reset");
        }

				#region LogicComponent Implementation
				public override string ComponentName => NotesAPIFactory.s_componentName;

				public NotesAPIcomponent(LiveSplitState state)
				{
            API_Thread.Start();
            m_state = state;
            m_state.OnStart += OnStart;
            m_state.OnSplit += OnSplit;
            m_state.OnUndoSplit += OnUndoSplit;
            m_state.OnSkipSplit += OnSkipSplit;
            m_state.OnReset += OnReset;
				}


				public override void Dispose()
        {
            API_Thread.Abort();
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
