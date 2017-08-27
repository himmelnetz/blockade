
var LEFT_KEY_KEYCODE = 37;
var RIGHT_KEY_KEYCODE = 39;

var PLAYER_END_POSITIONS = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

////////////////////////////////////////////////////////////////////////////////
// REMOTE BLOCKADE SERVICE /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

var RemoteBlockadeService = function(configuration_selector_component) {
	var play_one_game = function(on_success) {
		$.post(
			"/blockade/playOneGame",
			{
				data: JSON.stringify({
					Configuration: configuration_selector_component.get_configuration()
				})
			},
			function(data) {
				on_success(data)
			},
			"json");
	};
	
	var play_many_games = function(on_success) {
		$.post(
			"/blockade/playManyGames",
			{
				data: JSON.stringify({
					Configuration: get_configuration_from_selector(),
					NumGames: 1000
				})
			},
			function(data) {
				on_success(data);
			},
			"json");
	};
	
	return {
		play_one_game: play_one_game,
		play_many_games: play_many_games
	};
}

////////////////////////////////////////////////////////////////////////////////
// BLOCKADE BOARD COMPONENT ////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

var BlockadeBoardComponent = function() {
	// might want to SCREAMING_SNAKE these after eliminating the current screaming globals
	var Rows, Cols;
	
	var init_component = function() {
		// nothing right now
	}

	var reset_to_new_rows_cols = function(new_rows, new_cols) {
		Rows = new_rows;
		Cols = new_cols;
	
		// nuke whatever board exists and re-gen it
		$("#board-table").html("");
		var table = document.getElementById("board-table");
		for (var row = 0; row < Rows; row++) {
			var tr = document.createElement("tr");
			for (var col = 0; col < Cols; col++) {
				var td = document.createElement("td");
				td.id = "cell-" + row + "-" + col;
				tr.appendChild(td);
			}
			table.appendChild(tr);
		}
	};

	var set_cell_color = function(row, col, color1, color2, mix /* range is [0.0, 1.0] */, stateI) {
		var red = Math.round(color1[0] * (1.0 - mix) + color2[0] * mix);
		var green = Math.round(color1[1] * (1.0 - mix) + color2[1] * mix);
		var blue = Math.round(color1[2] * (1.0 - mix) + color2[2] * mix);
		var color = "rgb(" + red + ", " + green + ", " + blue + ")";
		$("#cell-" + row + "-" + col).css("background-color", color);
		_set_cell_border(row, col, "top", -1, 0, stateI);
		_set_cell_border(row, col, "bottom", 1, 0, stateI);
		_set_cell_border(row, col, "left", 0, -1, stateI);
		_set_cell_border(row, col, "right", 0, 1, stateI);
	};

	var _set_cell_border = function(row, col, direction, drow, dcol, stateI) {
		var thisTuple = _try_get_player_and_turn(row, col, stateI);
		var otherTuple = _try_get_player_and_turn(row + drow, col + dcol, stateI);
		var isBorderVisible = thisTuple !== null
			&& (otherTuple === null
				|| thisTuple.Item1 !== otherTuple.Item1
				|| Math.abs(thisTuple.Item2 - otherTuple.Item2) !== 1
				|| (Math.abs(thisTuple.Item2 - otherTuple.Item2) !== 1 && thisTuple.Item2 < otherTuple.Item2));
		var cellElement = $("#cell-" + row + "-" + col);
		if (isBorderVisible) {
			cellElement.css("border-" + direction, "2px solid black")
				.css("padding-" + direction, "0px");
		} else {
			cellElement.css("padding-" + direction, "2px")
				.css("border-" + direction, "0px");
		}
	};

	var _try_get_player_and_turn = function(row, col, stateI) {
		if (row < 0 || row >= NUM_ROWS || col < 0 || col >= NUM_COLS) {
			return null;
		}
		var tuple = BOARD[row][col];
		if (tuple === null) {
			return null;
		}
		return tuple.Item2 <= stateI ? tuple : null;
	};
	
	return {
		init_component: init_component,
		reset_to_new_rows_cols: reset_to_new_rows_cols,
		set_cell_color: set_cell_color
	};
}

////////////////////////////////////////////////////////////////////////////////
// GAME VIEWER COMPONENT ///////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

var GameViewerComponent = function(remote_blockade_service, blockade_board_component) {
	var init_component = function() {
		$("#play-new-game-button").button()
			.click(function(event) {
				_on_play_new_game_button();
			});
		_reset_slider();
	};
	
	var _on_play_new_game_button = function() {
		remote_blockade_service.play_one_game(function(data) {
			BOARD = data.Board;
			RESULTS_WITH_FINAL_TURN = data.ResultsWithFinalTurn;
			NUM_ROWS = BOARD.length;
			NUM_COLS = BOARD[0].length;
			blockade_board_component.reset_to_new_rows_cols(NUM_ROWS, NUM_COLS);
			_reset_to_new_game();
		});
	};
	
	var _reset_to_new_game = function() {
		// need to re-init slider so it has the right range
		_reset_slider();
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
	    _set_to_state(0, true);
	};
	
    var _on_slider_change = function() {
	    var stateI = $("#state-slider").slider("value");
	    _set_to_state(stateI, false);
	};
	
	var _reset_slider = function() {
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
	        slide: _on_slider_change,
	        change: _on_slider_change
	    });
	};
	
	var _set_to_state = function(stateI, change_slider) {
	    for (var row = 0; row < NUM_ROWS; row++) {
	        for (var col = 0; col < NUM_COLS; col++) {
	            var tuple = BOARD[row][col];
	            blockade_board_component.set_cell_color(row, col, [255, 255, 255], [0, 0, 0], 0.0, stateI);
	            if (tuple !== null && tuple.Item2 <= stateI) {
	                var mix = Math.min((Math.min(stateI, PLAYER_END_POSITIONS[tuple.Item1]) - tuple.Item2) / 15.0, 1.0);
	                blockade_board_component.set_cell_color(row, col, COLORS[tuple.Item1][0], COLORS[tuple.Item1][1], mix, stateI)
	            }
	        }
	    }
	    if (change_slider) {
	        $("#state-slider").slider({
	            value: stateI
	        });
	    }
	    for (var resultI = 0; resultI < RESULTS_WITH_FINAL_TURN.length; resultI++) {
	    	var playerI = RESULTS_WITH_FINAL_TURN[resultI].Item1;
	    	var turn = RESULTS_WITH_FINAL_TURN[resultI].Item2;
	    	if (turn <= stateI) {
	    		$("#result-td-" + resultI).html($("input[name=player-" + playerI + "-radio-group]:checked").val())
	    			.css("background-color", $("#player-" + playerI + "-color").css("background-color"));
	    		$("#result-td-survival-" + resultI).html(turn);
	    	} else {
	    		$("#result-td-" + resultI).html("")
	    			.css("background-color", "white");
	    		$("#result-td-survival-" + resultI).html("");
	    	}
	    }
	}
	
	return {
		init_component: init_component
	};
};

////////////////////////////////////////////////////////////////////////////////
// CONFIGURATION SELECTOR COMPONENT ////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

var ConfigurationSelectorComponent = function() {
	var init_component = function() {
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
	};
	
	var get_configuration = function() {
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
		};
	};

	return {
		init_component: init_component,
		get_configuration: get_configuration
	};
}

////////////////////////////////////////////////////////////////////////////////
// PLAY MANY GAMES COMPONENT ///////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

var PlayManyGamesComponent = function(remote_blockade_service) {
	var init_component = function() {
		$("#play-many-games-button").button()
			.click(function(event) {
				_on_play_many_games_button();
			});
	};
	
	var _on_play_many_games_button = function() {
		remote_blockade_service.play_many_games(function(data) {
			console.log(data);
		});
	};
	
	return {
		init_component: init_component
	};
}

////////////////////////////////////////////////////////////////////////////////
// BLOCKADE PAGE ///////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

var BlockadePage = function() {
	var configuration_selector_component = ConfigurationSelectorComponent();
	var remote_blockade_service = RemoteBlockadeService(configuration_selector_component);
	var blockade_board_component = BlockadeBoardComponent();
	var game_viewer_component = GameViewerComponent(remote_blockade_service, blockade_board_component);
	var play_many_games_component = PlayManyGamesComponent(remote_blockade_service);
	
	var _init_page = function() {
		configuration_selector_component.init_component();
		game_viewer_component.init_component();
		blockade_board_component.init_component();
		play_many_games_component.init_component();
	}
	
	var on_document_ready = function() {
		_init_page();
		$("#play-new-game-button").trigger("click");
	};
	
	return {
		on_document_ready: on_document_ready
	};
}

////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

$(document).ready(function() {
	BOARD = [[]];
	NUM_ROWS = 0;
	NUM_COLS = 0;

	var blockade_page = BlockadePage();
	blockade_page.on_document_ready();
});
