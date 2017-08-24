
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
		{
			data: JSON.stringify(get_configuration_from_selector())
		},
		function(data) {
			BOARD = data.Board;
			NUM_ROWS = BOARD.length;
			NUM_COLS = BOARD[0].length;
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
	$("#rows-slider").slider({
		range: "min",
		min: 10,
		max: 30,
		value: 10,
		slide: function(event, ui) {
			$("#rows-selector>span").html(ui.value);
		}
	});
	$("#cols-slider").slider({
		range: "min",
		min: 10,
		max: 40,
		value: 10,
		slide: function(event, ui) {
			$("#cols-selector>span").html(ui.value);
		}
	});
}

function initialize_to_new_game() {
	reset_board_table_dom(NUM_ROWS, NUM_COLS);
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

function reset_board_table_dom(rows, cols) {
	// nuke whatever board exists and re-gen it
	$("#board-table").html("");
	var table = document.getElementById("board-table");
	for (var row = 0; row < NUM_ROWS; row++) {
		var tr = document.createElement("tr");
		for (var col = 0; col < NUM_COLS; col++) {
			var td = document.createElement("td");
			td.id = "cell-" + row + "-" + col;
			tr.appendChild(td);
		}
		table.appendChild(tr);
	}
}

function get_configuration_from_selector() {
	var rows = parseInt($("#rows-selector>span").html());
	var cols = parseInt($("#cols-selector>span").html());
	return {
		Rows: rows,
		Cols: cols,
		PlayersWithStartingPosition: [
			{ Name: $("input[name=player-0-radio-group]:checked").val(), StartingLocation: { Item1: 0, Item2: 0 }},
			{ Name: $("input[name=player-1-radio-group]:checked").val(), StartingLocation: { Item1: rows - 1, Item2: cols - 1 }},
			{ Name: $("input[name=player-2-radio-group]:checked").val(), StartingLocation: { Item1: rows - 1, Item2: 0 }},
			{ Name: $("input[name=player-3-radio-group]:checked").val(), StartingLocation: { Item1: 0, Item2: cols - 1 }}
		]
	}
}

$(document).ready(function() {
    // TODO MOVE THIS SOMEHOW
	$.post(
		"/blockade/playOneGame",
		{
			data: JSON.stringify(get_configuration_from_selector())
		},
		function(data) {
			BOARD = data.Board;
			NUM_ROWS = BOARD.length;
			NUM_COLS = BOARD[0].length;
			initialize_to_new_game();
			init_page(); // THIS IS SPECIAL!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		},
		"json");
    
});
