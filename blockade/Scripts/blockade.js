
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
					Configuration: configuration_selector_component.get_configuration(),
					NumGames: 100
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

var BlockadeBoardComponent = function(color_provider) {
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
	
	var set_to_board = function(board, stateI, player_end_positions) {
	    for (var row = 0; row < board.length; row++) {
	        for (var col = 0; col < board[0].length; col++) {
	        	var player = board[row][col].player;
	        	var turn = board[row][col].turn;
	        	var mix = player !== -1 && turn <= stateI
	        		? Math.min((Math.min(stateI, player_end_positions[player]) - turn) / 15.0, 1.0)
	        		: 0.0;
	        	var color = turn <= stateI
					? color_provider.get_player_mix_color_rgb_string(player, mix)
					: "white";
				$("#cell-" + row + "-" + col).css("background-color", color);
				_set_cell_border(board, row, col, "top", -1, 0, stateI);
				_set_cell_border(board, row, col, "bottom", 1, 0, stateI);
				_set_cell_border(board, row, col, "left", 0, -1, stateI);
				_set_cell_border(board, row, col, "right", 0, 1, stateI);
	        }
	    }
	}

	var _set_cell_border = function(board, row, col, direction, drow, dcol, stateI) {
		var thisCell = _try_get_player_and_turn(board, row, col, stateI);
		var otherCell = _try_get_player_and_turn(board, row + drow, col + dcol, stateI);
		var isBorderVisible = thisCell.player !== -1
			&& (otherCell.player === -1
				|| thisCell.player !== otherCell.player
				|| Math.abs(thisCell.turn - otherCell.turn) !== 1
				|| (Math.abs(thisCell.turn - otherCell.turn) !== 1 && thisCell.turn < otherCell.turn));
		var cellElement = $("#cell-" + row + "-" + col);
		if (isBorderVisible) {
			cellElement.css("border-" + direction, "2px solid black")
				.css("padding-" + direction, "0px");
		} else {
			cellElement.css("padding-" + direction, "2px")
				.css("border-" + direction, "0px");
		}
	};

	var _try_get_player_and_turn = function(board, row, col, stateI) {
		if (row < 0 || row >= Rows || col < 0 || col >= Cols) {
			return { player: -1, turn: -1 };
		}
		return board[row][col].turn <= stateI
			? board[row][col]
			: { player: -1, turn: -1 };
	};
	
	return {
		init_component: init_component,
		reset_to_new_rows_cols: reset_to_new_rows_cols,
		set_to_board: set_to_board
	};
}

////////////////////////////////////////////////////////////////////////////////
// GAME VIEWER COMPONENT ///////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

var GameViewerComponent = function(remote_blockade_service, blockade_board_component, color_provider) {
	var Board = [[]];
	var Result_With_Final_Turn = [];
	// like in other places, this is currently hard coded to 4
	var Player_End_Positions = [0, 0, 0, 0];

	var init_component = function() {
		$("#play-new-game-button").button()
			.click(function(event) {
				_on_play_new_game_button();
			});
		_reset_slider();
	};
	
	var _on_play_new_game_button = function() {
		remote_blockade_service.play_one_game(function(data) {
			Board = _apply_func_over_board(data.Board, function(tuple) {
			   		return {
			   			player: tuple !== null ? tuple.Item1 : -1,
			   			turn: tuple !== null ? tuple.Item2 : -1
			   		};
			   	});
			Result_With_Final_Turn = data.ResultsWithFinalTurn;
			blockade_board_component.reset_to_new_rows_cols(Board.length, Board[0].length);
			_reset_to_new_game();
		});
	};
	
	var _reset_to_new_game = function() {
		// need to re-init slider so it has the right range
		_reset_slider();
		for (var i = 0; i < Player_End_Positions.length; i++) {
			Player_End_Positions[i] = 0;
		}
		_apply_func_over_board(Board, function(cell) {
			if (cell.player !== -1) {
				Player_End_Positions[cell.player] = Math.max(Player_End_Positions[cell.player], cell.turn);
			}
		});
	    _set_to_state(0, true);
	};
	
    var _on_slider_change = function() {
	    var stateI = $("#state-slider").slider("value");
	    _set_to_state(stateI, false);
	};
	
	var _reset_slider = function() {
	    var maxI = 0;
	    _apply_func_over_board(Board, function(cell) {
	    	if (cell.player !== -1) {
	    		maxI = Math.max(maxI, cell.turn)
	    	}
	    });
	    $("#state-slider").slider({
	        max: maxI,
	        slide: _on_slider_change,
	        change: _on_slider_change
	    });
	};
	
	var _apply_func_over_board = function(board, func) {
		var newBoard = [];
		for (var row = 0; row < board.length; row++) {
			var newRow = [];
			for (var col = 0; col < board[0].length; col++) {
				newRow.push(func(board[row][col]));
			}
			newBoard.push(newRow);
		}
		return newBoard;
	}
	
	var _set_to_state = function(stateI, change_slider) {
	   	blockade_board_component.set_to_board(Board, stateI, Player_End_Positions);
	    if (change_slider) {
	        $("#state-slider").slider({
	            value: stateI
	        });
	    }
	    for (var resultI = 0; resultI < Result_With_Final_Turn.length; resultI++) {
	    	var player_i = Result_With_Final_Turn[resultI].Item1;
	    	var turn = Result_With_Final_Turn[resultI].Item2;
	    	if (turn <= stateI) {
	    		$("#result-td-" + resultI).html($("input[name=player-" + player_i + "-radio-group]:checked").val())
	    			.css("background-color", color_provider.get_player_main_color_rgb_string(player_i));
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
			_set_to_new_win_percentages(data.WinPercentages);
		});
	};
	
	var _set_to_new_win_percentages = function(win_percentages) {
		for (var playerI = 0; playerI < win_percentages.length; playerI++) {
			for (var resultI = 0; resultI < win_percentages[0].length; resultI++) {
				$("#play-many-games-result-player-" + playerI + "-result-" + resultI)
					.html(Math.round(win_percentages[playerI][resultI] * 100) + "%");
			}
		}
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
	var color_provider = ColorProvider();
	var blockade_board_component = BlockadeBoardComponent(color_provider);
	var game_viewer_component = GameViewerComponent(remote_blockade_service, blockade_board_component, color_provider);
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
	var blockade_page = BlockadePage();
	blockade_page.on_document_ready();
});
