# SpaceBattle

Space Battle –game draft. The game is similar to the Space Invaders –game, where a group of enemies are approaching the player ship and the player should try to destroy the enemies before they reach the player ship.
The game is developed with Visual Studio 2017, C# / WPF. 

The game screen consists of seven overlapping canvases, each with their own purpose, i.e. enemy movement/animation, player ship, ammunition, explosion effects, player explosion/damage effect, background star field and sore/information texts.

The game runs in a GameLoop –thread which controls the game events and launches any explosion effect animations (which are run in their own dedicated thread(s)). The player keypresses are monitored application wide.

TODO
Graphics and logic for covers / shields could be added (the player ship could take cover behind them), which would hold for couple of enemy ammunition hits.