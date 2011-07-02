
using Manos;

using System;
using System.IO;

namespace Moo 
{
	public class Moo : ManosApp 
	{
		const string baseCowDirectory = "/home/jeremie/cows/";

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
			ctx.Response.End (cow);
		}

		string ProxyCowsay (string cowPath, string face, string isThink, string columns, string message)
		{
			cowPath = baseCowDirectory + cowPath;
			if (!File.Exists (cowPath))
				cowPath = Cowsay.DefaultCowPath;

			Cowsay.Faces f;
			if (string.IsNullOrEmpty (face) || !Enum.TryParse<Cowsay.Faces> (face, out f))
				f = Cowsay.Faces.Default;

			bool isTk = !string.IsNullOrEmpty (isThink) && isThink.Equals ("true", StringComparison.Ordinal);

			int cols;
			if (string.IsNullOrEmpty (face) || !int.TryParse (columns, out cols))
				cols = 40;

			message = message.Trim ();
			message = string.IsNullOrEmpty (message) ? "Moo powered by Manos" : message;

			return Cowsay.GetIt (cowPath, f, isTk, cols, message);
		}

		void HtmlServing (IManosContext ctx, string htmlPath)
		{
			staticContent.Content (ctx, htmlPath);
		}
	}
}