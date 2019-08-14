using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardMaker
{
    public class CardTypeEntry : IComparable<string>
    {
        public CardTypeEntry()
        {
        }

        public CardTypeEntry(string val)
        {
            Value = val;
        }

        public string Value { get; set; }


        public int CompareTo(string other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(CardTypeEntry other)
        {
            return Value.Equals(other.Value);
        }

        override public string ToString()
        {
            return Value;
        }
    }
}