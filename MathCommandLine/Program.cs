using IML.Commands;
using IML.CoreDataTypes;
using IML.Environments;
using IML.Evaluation;
using IML.Exceptions;
using IML.Functions;
using IML.Structure;
using IML.Syntax;
using IML.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace IML
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
