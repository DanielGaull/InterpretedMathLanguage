using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IML.Parsing.Util
{
    public class WrapperLevels
    {
        Dictionary<string, int> levels;
        bool inString;

        public WrapperLevels()
        {
            levels = new Dictionary<string, int>();
            inString = false;
        }

        public bool IsInString()
        {
            return inString;
        }

        public void SetInString(bool value)
        {
            inString = value;
        }

        public int GetLevel(string wrapper)
        {
            if (levels.ContainsKey(wrapper))
            {
                return levels[wrapper];
            }
            return 0;
        }

        public void ChangeLevel(string wrapper, int amount)
        {
            if (levels.ContainsKey(wrapper))
            {
                levels[wrapper] += amount;
            }
            else
            {
                levels.Add(wrapper, amount);
            }
        }

        public bool AtLevelZero()
        {
            return levels.Values.All(x => x == 0) && !inString;
        }
    }
}
