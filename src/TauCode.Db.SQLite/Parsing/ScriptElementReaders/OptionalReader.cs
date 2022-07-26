using System;
using TauCode.Parsing.Graphs.Molding;
using TauCode.Parsing.Graphs.Molding.Impl;
using TauCode.Parsing.Graphs.Reading;
using TauCode.Parsing.Graphs.Reading.Impl;
using TauCode.Parsing.TinyLisp.Data;

namespace TauCode.Db.SQLite.Parsing.ScriptElementReaders
{
    public class OptionalReader : GroupReader
    {
        public OptionalReader(IGraphScriptReader scriptReader)
            : base(scriptReader)
        {
        }

        protected override void CustomizeContent(IScriptElementMold scriptElementMold, Element element)
        {
            var groupMold = (GroupMold)scriptElementMold;

            var linkables = groupMold.Linkables;
            if (linkables.Count != 1)
            {
                throw new NotImplementedException("error: optional wants exactly one child");
            }

            var linkable = linkables[0];
            var optionalEntrance = new VertexMold(groupMold, Symbol.Create("idle"))
            {
                IsEntrance = true,
            };
            if (groupMold.Name != null)
            {
                optionalEntrance.Name = $"*entrance-for-{groupMold.Name}";
            }


            var optionalExit = new VertexMold(groupMold, Symbol.Create("idle"))
            {
                IsExit = true,
            };
            if (groupMold.Name != null)
            {
                optionalExit.Name = $"*exit-for-{groupMold.Name}";
            }

            optionalEntrance.AddLinkTo(linkable);
            optionalEntrance.AddLinkTo(optionalExit);

            linkable.AddLinkTo(optionalExit);

            groupMold.Add(optionalEntrance);
            groupMold.Add(optionalExit);
        }
    }
}
