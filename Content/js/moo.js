
setTimeout (function () {
	(function($){
		var input = $('#input');
		var cow = $('#cow');

		input.keypress (function (event) {
			if (event.which != 13)
				return;

			event.preventDefault();
			$.post ('/moo', { message: input.val() }, function (data) {
				cow.text (data);
			}, 'text');
		});
	})(window.jQuery);
}, 2);

