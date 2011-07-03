
setTimeout (function () {
	(function($){
		var input = $('#input');
		var cow = $('#cow');
		var type = $('#type');
		var permacow = $('#permacow');

		var process = function () {
			var postdata = { message: input.val(), cowfile: type.val () };

			permacow.attr('href', '/?permacow=' + escape(btoa(JSON.stringify(postdata))));

			$.post ('/moo', postdata, function (data) {
				cow.text (data);
			}, 'text');
		};

		input.keypress (function (event) {
			if (event.which != 13)
				return;

			event.preventDefault();
			process ();
		});
		type.change(function (event) {
			process ();
		});

		if (window.location.search.length > 1) {
			var is, as = window.location.search.substr(1).split("&");
			for (var i = 0; i < as.length; i++) {
				is = as[i].split("=");
				if (is.length != 2 || is[0] != 'permacow')
					continue;

				try {
					var parsed = JSON.parse(atob(unescape(is[1])));
					input.val(parsed.message);
					type.val(parsed.cowfile);
					process ();
				} catch (e) { }
			}
		}
	})(window.jQuery);
}, 2);

