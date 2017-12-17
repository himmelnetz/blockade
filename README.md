# blockade
C# implementation of [Blockade](https://en.wikipedia.org/wiki/Blockade_(video_game)) for making AIs play against each other. Games are played out entirely in C# with a viewer written in html/js.

The current best AI performs a heuristic search looking forwards after everyones next move. For its next move it scores state with a heuristic and for opponent's moves it averages all possible moves' heuristic score. The heuristic prefers moving towards areas with more available cells, with ties broken by avoiding occupied neighbor cells. In a 15x15 match against 3 other AIs playing slightly better than random, this best AI wins ~60% of the time, coming in 1st or 2nd ~90% of the time.

Sample screeshot of the viewer:
![viewer screenshot](https://github.com/himmelnetz/blockade/blob/master/viewer-screenshot.png)

Note: This was developed in Linux using Mono, so there can be a few quirks to the repo, eg how MVC works and why some packages are committed.
