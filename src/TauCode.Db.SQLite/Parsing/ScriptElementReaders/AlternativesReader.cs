using TauCode.Parsing.Graphs.Molding;
using TauCode.Parsing.Graphs.Molding.Impl;
using TauCode.Parsing.Graphs.Reading;
using TauCode.Parsing.Graphs.Reading.Impl;
using TauCode.Parsing.TinyLisp.Data;

namespace TauCode.Db.SQLite.Parsing.ScriptElementReaders;

public class AlternativesReader : GroupReader
{
    public AlternativesReader(IGraphScriptReader scriptReader)
        : base(scriptReader)
    {
    }

    protected override void CustomizeContent(IScriptElementMold scriptElementMold, Element element)
    {
        var alternativesGroupMold = (GroupMold)scriptElementMold;

        var entrance = new VertexMold(alternativesGroupMold, Symbol.Create("idle"))
        {
            IsEntrance = true,
        };
        alternativesGroupMold.Add(entrance);

        var exit = new VertexMold(alternativesGroupMold, Symbol.Create("idle"))
        {
            IsExit = true,
        };
        alternativesGroupMold.Add(exit);

        for (var i = 0; i < alternativesGroupMold.Linkables.Count - 2; i++)
        {
            var innerLinkable = alternativesGroupMold.Linkables[i];

            entrance.AddLinkTo(innerLinkable);

            innerLinkable.AddLinkTo(exit);
        }
    }
}