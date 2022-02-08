using LiveSplit.NotesCreatorAPI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveSplit.Model;

[assembly: ComponentFactory(typeof(NotesAPIFactory))]

namespace LiveSplit.NotesCreatorAPI
{
    class NotesAPIFactory : IComponentFactory
    {

        public const string s_componentName = "NotesCreatorAPI";
        private const string s_componentDesc = "A small tool to connect LiveSplitter and NotesCreator.";

        private const string s_updateURL = "https://github.com/iNightfaller/SpeedGuidesLive";

        #region IComponentFactory Interface
        public string ComponentName { get { return s_componentName; } }
        public string Description { get { return s_componentDesc; } }
        public ComponentCategory Category { get { return ComponentCategory.Other; } }
        public string UpdateName { get { return "TODO: UpdateName"; } }
        public string XMLURL { get { return "TODO: XmlURL"; } }
        public string UpdateURL { get { return "TODO: s_updateURL"; } }
        public Version Version { get { return Version.Parse("1.0.0"); } }
        public IComponent Create(LiveSplitState state) { return new NotesAPIcomponent(state); }
        #endregion
    }
}
