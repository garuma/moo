
using Manos;
using Manos.Http;

using System;
using System.IO;
using System.Text;

namespace Moo 
{
	public class Moo : ManosApp 
	{
		static readonly string baseCowDirectory = Path.GetFullPath ("cows");
		static readonly Encoding encoding = new UTF8Encoding ();

		StaticContentModule staticContent;
		string indexPath = Path.Combine ("Content", "html", "index.html");

		public Moo ()
		{
			staticContent = new StaticContentModule ("Content");
			Route ("/Content/", staticContent);
		}

		[Route ("/")]
		public void Index (IManosContext ctx)
		{
			HtmlServing (ctx, indexPath);
		}

		[Route ("/moo")]
		public void MakeMoo (IManosContext ctx)
		{
			string cowPath = ctx.Request.Data["cowfile"];
			string face = ctx.Request.Data["face"];
			string isThink = ctx.Request.Data["isThink"];
			string columns = ctx.Request.Data["columns"];
			string message = ctx.Request.Data["message"] ?? string.Empty;

			string cow = ProxyCowsay (cowPath, face, isThink, columns, message);
			ctx.Response.ContentEncoding = encoding;
			ctx.Response.End (cow);
		}

		string ProxyCowsay (string cowPath, string face, string isThink, string columns, string message)
		{
			if (string.IsNullOrEmpty (cowPath) || !ValidFile (cowPath = Path.Combine (baseCowDirectory, cowPath)))
				cowPath = Cowsay.DefaultCowPath;

			Cowsay.Faces f;
			if (string.IsNullOrEmpty (face) || !Enum.TryParse<Cowsay.Faces> (face, out f))
				f = Cowsay.Faces.Default;

			bool isTk = !string.IsNullOrEmpty (isThink) && isThink.Equals ("true", StringComparison.Ordinal);

			int cols;
			if (string.IsNullOrEmpty (face) || !int.TryParse (columns, out cols))
				cols = 40;

			message = message.Trim ();
			message = string.IsNullOrEmpty (message) ? "Moo powered by Manos" : HttpUtility.HtmlDecode (message);

			return Cowsay.GetIt (cowPath, f, isTk, cols, message);
		}

		void HtmlServing (IManosContext ctx, string htmlPath)
		{
			staticContent.Content (ctx, htmlPath);
		}

		private bool ValidFile (string path)
		{
			try {
				string full = Path.GetFullPath (path);
				if (full.StartsWith (baseCowDirectory))
					return File.Exists (full);
			} catch {
				return false;
			}

			return false;
		}
	}
}
