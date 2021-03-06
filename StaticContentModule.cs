//
// Copyright (c) 2010 Jérémie "garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Manos;
using Manos.Routing;

namespace Moo
{
	public class StaticContentModule : ManosModule
	{
		readonly HashSet<string> cacheExtensions;
		readonly string expires;
		readonly string cacheControl;
		readonly ConcurrentDictionary<string, string> etagCache;
		readonly FileSystemWatcher fsw;
		readonly string rootedPath;

		static readonly string[] defaultCachedExts = { ".html", ".js", ".css", ".png", ".jpg",
		                                               ".woff", ".eot", ".ttf", ".ico" };

		public StaticContentModule () : this (null)
		{
		}

		public StaticContentModule (string dir) : this (dir, defaultCachedExts, TimeSpan.FromDays (30))
		{
		}

		public StaticContentModule (string dir,
		                            IEnumerable<string> cacheExtensions,
		                            TimeSpan expireTime)
		{
			if (string.IsNullOrEmpty (dir))
				return;
			if (dir.StartsWith (Path.PathSeparator.ToString ()))
				dir = dir.Substring (1);

			this.cacheExtensions = new HashSet<string> (cacheExtensions);
			this.expires = (DateTime.Now + expireTime).ToString ("R");
			this.cacheControl = "max-age=" + ((int)expireTime.TotalSeconds).ToString ();
			this.etagCache = new ConcurrentDictionary<string, string> ();

			fsw = new FileSystemWatcher (dir);
			InitFileSystemWatcher (dir);
			rootedPath = Path.Combine (Environment.CurrentDirectory, dir);

			Get (".*", MatchType.Regex, Content);
		}

		void InitFileSystemWatcher (string dir)
		{
			fsw.NotifyFilter = NotifyFilters.LastWrite;
			fsw.IncludeSubdirectories = true;
			fsw.Changed += (obj, e) => etagCache[Path.Combine (dir, e.Name)] = GetEtagFromFile (e.FullPath);
			fsw.EnableRaisingEvents = true;
		}

		public void Content (IManosContext ctx)
		{
			Content (ctx, ctx.Request.Path);
		}

		public void Content (IManosContext ctx, string path)
		{
			if (path.StartsWith ("/"))
				path = path.Substring (1);

			string etag, fileEtag;
			if (cacheExtensions != null
			    && ctx.Request.Headers.TryGetNormalizedValue ("If-None-Match", out etag)
			    && etagCache.TryGetValue (path, out fileEtag)
			    && fileEtag.Equals (etag, StringComparison.Ordinal)) {

				ctx.Response.StatusCode = 304;
				ctx.Response.End ();
				return;
			}

			if (ValidFile (path)) {
				var mime = ManosMimeTypes.GetMimeType (path);
				if (mime.StartsWith ("text/", StringComparison.Ordinal) || mime.EndsWith ("javascript", StringComparison.Ordinal))
					mime += "; charset=utf-8";

				ctx.Response.Headers.SetNormalizedHeader ("Content-Type", mime);

				// Expires setting for specific files
				string extension = Path.GetExtension (path);
				if (cacheExtensions != null && !string.IsNullOrEmpty (extension) && cacheExtensions.Contains (extension)) {
					ctx.Response.Headers.SetNormalizedHeader ("Expires", expires);
					ctx.Response.Headers.SetNormalizedHeader ("Cache-Control", cacheControl);
					if (!etagCache.ContainsKey (path))
						etagCache[path] = GetEtagFromFile (path);
					ctx.Response.Headers.SetNormalizedHeader ("ETag", etagCache[path]);
				}

				ctx.Response.SendFile (path);
			} else
				ctx.Response.StatusCode = 404;

			ctx.Response.End ();
		}

		bool ValidFile (string path)
		{
			try {
				string fullPath = Path.GetFullPath (path);
				return fullPath.StartsWith (rootedPath);
			} catch {
				return false;
			}
		}

		static string GetEtagFromFile (string path)
		{
			return File.GetLastWriteTimeUtc (path).ToString ("s");
		}
	}
}

