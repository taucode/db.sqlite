using System;
using System.Collections.Generic;
using System.Linq;
using TauCode.Data.Graphs;
using TauCode.Parsing.Graphs.Building;
using TauCode.Parsing.Graphs.Molding;
using TauCode.Parsing.Nodes;
using TauCode.Parsing.TinyLisp.Data;

namespace TauCode.Db.SQLite.Parsing
{
    internal class SQLiteVertexFactory : IVertexFactory
    {
        public IVertex Create(IVertexMold vertexMold)
        {
            IVertex result;

            if (vertexMold.Car is StringAtom stringAtom)
            {
                var value = stringAtom.Value;
                if (value.Length == 1)
                {
                    result = new ExactPunctuationNode(value.Single());
                }
                else
                {
                    result = new ExactWordNode(value, true);
                }
            }
            else if (vertexMold.Car is Symbol symbol)
            {
                switch (symbol.Name)
                {
                    case "IDLE":
                        result = new IdleNode();
                        break;

                    case "END":
                        result = new EndNode();
                        break;

                    case "IDENTIFIER":
                        result = new SqlIdentifierNode(SQLiteParsingHelper.IsReservedWord);
                        break;

                    case "INTEGER":
                        result = new Int32Node();
                        break;

                    case "STRING":
                        result = new StringNode();
                        break;

                    case "MULTI-WORD":
                        result = new MultiWordNode((List<string>)vertexMold.GetKeywordValue(":VALUES"), true);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            result.Name = vertexMold.Name;
            return result;
        }
    }
}
