using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Moo
{
	public class Cowsay
	{
		public enum Faces {
			Default,
			Borg,
			Dead,
			Greedy,
			Paranoid,
			Stoned,
			Tired,
			Wired,
			Young
		}

		// Border definitions
		static readonly char[] thinkBorder = { '(', ')', '(', ')', '(', ')' };
		static readonly char[] smallBorder = { '<', '>' };
		static readonly char[] longBorder = { '/', '\\', '|', '|', '\\', '/', };

		public const string DefaultCowPath = "cows/default";

		static Dictionary<string, string> cowCache = new Dictionary<string, string> ();

		public static string GetIt (string cowPath, Faces face, bool isThink, int columns, string message)
		{
			string[] lines = Wrap (message, columns);
			string eyes = ConstructFace (face);
			string thoughts = isThink ? "o" : "\\";
			string tongue = "  ";

			StringBuilder sb = new StringBuilder ();
			ConstructBallon (lines, isThink, sb);
			sb.AppendLine (FetchAndFormatCow (cowPath ?? DefaultCowPath, thoughts, eyes, tongue));

			return sb.ToString ();
		}

		static string[] Wrap (string message, int columns)
		{
			List<string> lines = new List<string> ();
			StringBuilder sb = new StringBuilder (columns);

			int current = 0;
			for (int i = 0; i < message.Length; i++) {
				// Fetch next word length
				int nextIndex = message.IndexOf (' ', i);
				if (nextIndex == -1)
					nextIndex = message.Length;

				// In case someone input a really long word, update cols and rerun
				if (nextIndex - i >= columns)
					return Wrap (message, nextIndex - i + 1);

				if (current + (nextIndex - i) >= columns) {
					sb.Append (' ', columns - sb.Length);
					lines.Add (sb.ToString ());
					sb.Remove (0, sb.Length);
					current = 0;
					i--;
				} else {
					sb.Append (message, i, nextIndex - i);
					sb.Append (' ');
					current += nextIndex - i + 1;
					i = nextIndex;
				}
			}
			lines.Add (sb.ToString ());

			return lines.ToArray ();
		}

		static string ConstructFace (Faces face)
		{
			switch (face) {
			case Faces.Borg:
				return "==";
			case Faces.Dead:
				return "U ";
			case Faces.Greedy:
				return "$$";
			case Faces.Paranoid:
				return "@@";
			case Faces.Stoned:
				return "**";
			case Faces.Tired:
				return "--";
			case Faces.Wired:
				return "OO";
			case Faces.Young:
				return "..";
			default:
				return "oo";
			}
		}

		static void ConstructBallon (string[] lines, bool isThink, StringBuilder sb)
		{
			int maxlength = lines.Max (i => i.Length);
			int padded = maxlength + 2;

			char[] border = isThink ? thinkBorder : lines.Length < 2 ? smallBorder : longBorder;

			AddSpacer (sb, '_', padded);

			string format = "{0} {1,-" + maxlength + "} {2}\n";

			if (lines.Length == 1)
				sb.AppendFormat (format, border[0], lines[0], border[1]);
			else
				for (int i = 0; i < 3; i++)
					sb.AppendFormat (format, border[2 * i], i < lines.Length ? lines[i] : string.Empty, border[2 * i + 1]);

			AddSpacer (sb, '-', padded);
		}

		static string FetchAndFormatCow (string path, string thoughts, string eyes, string tongue)
		{
			string cow;
			if (!cowCache.TryGetValue (path, out cow)) {
				cow = File.ReadAllText (path);
				cowCache[path] = cow;
			}

			return cow.Replace ("$thoughts", thoughts).Replace ("$eyes", eyes).Replace ("$tongue", tongue);
		}

		static void AddSpacer (StringBuilder sb, char character, int times)
		{
			sb.Append (' ');
			sb.Append (character, times);
			sb.Append (' ');
			sb.AppendLine ();
		}
	}
}