XNA World Editor
================

2D world editor written in C#. The world view is rendered using XNA. I originally developed this tool in 2012 as a side project for an infinite side scrolling platformer game that I was working on. This is an iteration upon a hacked together editor using HTML5, which would honestly be a waste of time and sanity to put on GitHub.

The forms and UI components use [DotNetBar](http://www.devcomponents.com/dotnetbar/). Because DNB is not Open Sourced, I have stripped out all resource and project files and only the core classes that do things remain.

#### Features Include
* Import world from a file
* Export as image or XNA content file. Written in such a way that alternative file formats can easily be implemented
* Complete history management (undo/redo actions)
* Collision tiles
* Multi-layers
* Multi-tilesets
* Toggle tile grid
* Toggle layer opacity (selected layer is more opaque than other layers)
* Tabbed worlds (edit more than one at the same time)
###### Tools

* Tile Brush
* Tile Fill
* Tile Rectangle Selection


#### Screenshot

![Screenshot](http://i.imgur.com/XwyeZwy.png)

Tile assets can be found [here](http://www.photonstorm.com/flash-game-dev-tips/flash-game-dev-tip-12-building-a-retro-platform-game-in-flixel-part-1).
