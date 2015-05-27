using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map.Readers
{
    public class TokenResult
    {
        public char Token = '\0';
        public string Text;
    }

    public class Tokenizer
    {
        public static readonly char[] DEFAULT_EAT_TOKENS = { '\t', ' ', '\"', '\r', '\n' };
        public static readonly char[] DEFAULT_TOKENS = { '(', '{', '}', ')', '[', ']' };

        int sub;
        string data;
        char[] tokens;
        char[] eatChars;

        public Tokenizer(string data, char[] tokens, char[] eatChars)
        {
            this.tokens = tokens;
            this.eatChars = eatChars;
            this.data = data;
        }

        public TokenResult GetToken()
        {
            TokenResult ret = new TokenResult();

            StringBuilder sb = new StringBuilder();
            while (sub < data.Length)
            {
                // hit a real token
                if (tokens.Contains(data[sub]))
                {
                    sb.Append(data[sub]);
                    ret.Token = data[sub];
                    ret.Text = sb.ToString();
                    return ret;
                }
                //hit an 'eat' character
                else if (eatChars.Contains(data[sub])) 
                {
                    // if we have text then we stop
                    if (sb.Length > 0)
                    {
                        ret.Text = sb.ToString();
                        return ret;
                    }
                    // otherwise, we eat this character
                }
                else
                {
                    sb.Append(data[sub]);
                }
            }
            ret.Text = sb.ToString();
            return ret;
        }
    }
}
