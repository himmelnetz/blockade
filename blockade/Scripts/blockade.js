
var LEFT_KEY_KEYCODE = 37;
var RIGHT_KEY_KEYCODE = 39;

var COLORS = [
	[[21, 25, 81], [104, 112, 226]], // blue
	[[84, 20, 20], [244, 97, 97]], // red
	[[137, 135, 24], [214, 212, 102]], // yellow
	[[19, 76, 34], [92, 224, 127]], // green
	[[64, 26, 79], [191, 115, 221]], // purple
	[[122, 94, 39], [219, 165, 57]] // orange
];

var PLAYER_END_POSITIONS = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

function set_to_state(stateI, change_slider) {
    for (var row = 0; row < NUM_ROWS; row++) {
        for (var col = 0; col < NUM_COLS; col++) {
            var tuple = BOARD[row][col];
            set_cell_color(row, col, [255, 255, 255], [0, 0, 0], 0.0);
            if (tuple !== null && tuple.Item2 <= stateI) {
                var mix = Math.min((Math.min(stateI, PLAYER_END_POSITIONS[tuple.Item1]) - tuple.Item2) / 10.0, 1.0);
                set_cell_color(row, col, COLORS[tuple.Item1][0], COLORS[tuple.Item1][1], mix)
            }
        }
    }
    if (change_slider) {
        $("#state-slider").slider({
            value: stateI
        });
    }
}

function set_cell_color(row, col, color1, color2, mix /* range is [0.0, 1.0] */) {
	var red = Math.round(color1[0] * (1.0 - mix) + color2[0] * mix);
	var green = Math.round(color1[1] * (1.0 - mix) + color2[1] * mix);
	var blue = Math.round(color1[2] * (1.0 - mix) + color2[2] * mix);
	$("#cell-" + row + "-" + col).css("background-color", "rgb(" + red + ", " + green + ", " + blue + ")");
}

function on_slider_change() {
    var stateI = $("#state-slider").slider("value");
    set_to_state(stateI, false);
}

function on_play_new_game_button() {
	$.post(
		"/blockade/playOneGame",
		{ /* data goes here */ },
		function(data) {
			BOARD = data.Board;
			initialize_to_new_game();
		},
		"json");
}

function init_slider() {
    var maxI = 0;
    for (var row = 0; row < NUM_ROWS; row++) {
        for (var col = 0; col < NUM_COLS; col++) {
            if (BOARD[row][col] !== null) {
                maxI = Math.max(maxI, BOARD[row][col].Item2)
            }
        }
    }
    $("#state-slider").slider({
        max: maxI,
        slide: on_slider_change,
        change: on_slider_change
    });
}

function init_page() {
	init_slider();
	$("#play-new-game-button").button()
		.click(function(event) {
			on_play_new_game_button();
		});
}

function initialize_to_new_game() {
	// need to re-init slider so it has the right range
	init_slider();
	for (var i = 0; i < PLAYER_END_POSITIONS.length; i++) {
		PLAYER_END_POSITIONS[i] = 0;
	}
    for (var row = 0; row < NUM_ROWS; row++) {
        for (var col = 0; col < NUM_COLS; col++) {
            var tuple = BOARD[row][col];
            if (tuple != null) {
            	var player = tuple.Item1;
            	var turn = tuple.Item2;
            	PLAYER_END_POSITIONS[player] = Math.max(PLAYER_END_POSITIONS[player], turn);
            }
        }
    }
    set_to_state(0, true);
}

$(document).ready(function() {
    init_page();
    initialize_to_new_game();
});
