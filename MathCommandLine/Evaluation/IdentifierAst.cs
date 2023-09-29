using System;
using System.Collections.Generic;
using System.Text;

namespace MathCommandLine.Evaluation
{
    public class IdentifierAst
    {
        public IdentifierAstTypes Type { get; private set; }
        // These two are only used for the DotAccess type
        public Ast Parent { get; private set; }
        // Name used for RawVar and Parent
        public string Name { get; private set; }
        // Used for Dereference types
        public Ast Reference { get; private set; }

        private IdentifierAst(IdentifierAstTypes type, Ast parent, string name, Ast reference)
        {
            Type = type;
            Parent = parent;
            Name = name;
            Reference = reference;
        }

        public static IdentifierAst RawVariable(string name)
        {
            return new IdentifierAst(IdentifierAstTypes.RawVar, null, name, null);
        }
        public static IdentifierAst MemberAccess(Ast parent, string name)
        {
            return new IdentifierAst(IdentifierAstTypes.MemberAccess, parent, name, null);
        }
        public static IdentifierAst Dereference(Ast reference)
        {
            return new IdentifierAst(IdentifierAstTypes.Dereference, null, null, reference);
        }
    }

    public enum IdentifierAstTypes
    {
        RawVar,
        MemberAccess,
        Dereference,
    }
}
