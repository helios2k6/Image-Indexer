/* 
 * Copyright (c) 2015 Andrew Johnson
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
 * Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;

namespace Core.Console
{
    public static class ConsoleUtils
    {
        /// <summary>
        /// Get an argument's values
        /// </summary>
        /// <param name="args">The argument array</param>
        /// <param name="argSwitch">The argument statement</param>
        /// <returns>An IEnumerable of all of the argument's values</returns>
        public static IEnumerable<string> GetArgumentTuple(string[] args, string argSwitch)
        {
            var values = new List<string>();
            bool shouldCollect = false;
            foreach (string arg in args)
            {
                if (string.Equals(argSwitch, arg, System.StringComparison.Ordinal))
                {
                    shouldCollect = true;
                    continue;
                }

                if (shouldCollect)
                {
                    if (arg.IndexOf("--") == 0)
                    {
                        break;
                    }
                    values.Add(arg);
                }
            }

            return values;
        }
    }
}