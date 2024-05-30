using System;
using System.Collections.Generic;
using System.Text;

namespace IML.Parsing.AST.ValueAsts
{
    public enum AstTypes
    {
        // A literal number, such as 5, -10.2, or 0.6
        NumberLiteral,
        // A literal list, in the format of { [element0], [element1], [element2], ... }
        ListLiteral,
        // A literal lamda, in the format of ([params])=>{[body]}
        LambdaLiteral,
        // A literal string, in the format of "[text]"
        StringLiteral,
        // A literal reference, in the format of "&[varname]"
        ReferenceLiteral,
        // A literal variable, which must be made up of valid characters (A-Z, a-z, 0-9, _)
        // Can only start with a letter
        Variable,
        // Used when accessing a member of an object
        // Ex. "str".chars
        MemberAccess,
        // An instance in which an invokation is being performed on some other object
        // i.e. _add(1, 2)
        // Format is: [callee]([arg0],[arg1],...)
        Call,
        // A variable declaration
        // Example: var x: number = 7
        VariableDeclaration,
        // A variable assignment
        // May or may not include a binary operator, too
        // Example: x = 7; x += 5; b &&= a || c;
        VariableAssignment,
        // A "return" statement for use within functions
        // Example: return 7+2;
        Return,
        // Anything that doesn't fit into the above definitions
        // Used in syntax handling, but if it appears when parsing some final expression,
        // then there's an error
        Invalid,
    }
}
