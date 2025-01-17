/* Created by SharpDevelop.
   User: Marc365
   Date: 05/05/2024
   Time: 18:43
   Update: 19/05/2024
 */

using NSox.NAudio;
using System;

namespace NSox
{
	class Help
	{
        internal static void Write(string s)
        {
            Console.Write(s);
        }

        internal static void WriteLine(string s)
        {
            Console.WriteLine(s);
        }

        internal static void Show()
        {
            Version();
            WriteLine(Def.NPlay_Help);
        }

        internal static void Version()
        {
            WriteLine(Def.NPlay_Copyright);
        }

        internal static void Fatal(Exception exc)
        {
            Write(exc.Message); Write(Def.Space); WriteLine(exc.StackTrace);
            System.Environment.Exit(0);
        }
    }
}
