using MathCommandLine.Commands;
using MathCommandLine.CoreDataTypes;
using MathCommandLine.Environments;
using MathCommandLine.Evaluation;
using MathCommandLine.Exceptions;
using MathCommandLine.Functions;
using MathCommandLine.Structure;
using MathCommandLine.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MathCommandLine
{
    class Program
    {
        
        static void Main(string[] args)
        {
            CommandHandler cmdHandler = new CommandHandler();
            cmdHandler.Run(args);
        }
    }
}
