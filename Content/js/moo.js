
setTimeout (function () {
	(function($){
		var input = $('#input');
		var cow = $('#cow');
		var type = $('#type');

		var process = function () {
			$.post ('/moo', { message: input.val(), cowfile: type.val () }, function (data) {
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
	})(window.jQuery);
}, 2);

