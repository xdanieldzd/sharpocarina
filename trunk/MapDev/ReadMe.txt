------------------------------------------------------
SharpOcarina v0.2 - Zelda OoT Scene Development System
              Written in 2011 by xdaniel              
------------------------------------------------------

--------------------
0) Table of Contents
--------------------
 1) System Requirements
 2) User Interface
  a) Menus
  b) General Scene Data
  c) Transitions & Spawns
  d) Collision & Exits
  e) Rooms & Group Settings
  f) Objects & Actors
 3) FAQ
 4) Credits & Thanks

----------------------
1) System Requirements
----------------------
 ...are modest. No, really, it should run on pretty much every kinda Windows PC out there. Unlike
 SayakaGL or OZMAV2, it doesn't require any fancy shader extensions and such (yet?) and thus the
 rendering of the scene preview should be glitch-free on anything that's reasonably modern. What it
 -does- require is the most recent version of Microsoft's .NET Framework 4, which, in case you
 haven't installed it yet, can be found in their download center.

-----------------
2) User Interface
-----------------
 The GUI might be a bit intimidating at first, with all the options and stuff it has, but it's bound
 to become second nature once you've used it for awhile. There's some quirks to the interface,
 though. For instance, with most text boxes you have to press -Enter- after entering your offset,
 actor number, what have you. If you don't do this, your changes will not be committed to the scene.
 Also, outside of camera movement, there is no mouse control in the 3D preview at all, so you cannot
 ex. select and drag actors with the mouse.
 
 a) Menus
 --------
  | File |
   - New Scene: Creates a new, mostly empty scene. It does automatically add environment settings
                for all basic occasions (all times, normal, underwater, rainy), a spawn point for
                Link at coordinates 0/0/0 and a simple ground collision type.

   - Open Scene: Opens a previously saved scene XML and loads all associated data (model files,
                 etc.).
   
   - Save Scene: Saves the current scene to an XML file.
   
   - Save Binary: Asks for a target directory, converts the current scene into separate scene and
                  room files and saves them to the selected directory.

   - Inject to ROM: Converts the current scene and injects it into the given ROM. Remember to set
                    the proper injection offsets for the scene and each room! Also remember that
                    this -does not- check if it's overwriting existing data in the ROM!

   - Exit: Self-explanatory, I hope.

  | Options |
   - Show Collision Model: When checked, you'll get a transparent red rendering of the collision
                           model overlayed on top of the room models.

   - Show Room Models: The room models will only be rendered when this is checked. Might be useful
                       to uncheck if you want to look at just the collision.

   - Apply Environment Lighting: Gives a very rough representation of lighting when checked, by far
                                 not akin to how it will look in-game. Just leave it off.

   - Consecutive Room Injection: When this is checked, in a multi-room scene, you won't need to
                                 specify injection offsets for any subsequent rooms beyond the
                                 first. They will simply be injected right after the previous one.
                                 Although, just like the ROM injection function in general, this
                                 -does not- check if it overwrites any existing data.

  | Help |
   - About...: Take a wild guess.

 b) General Scene Data
 ---------------------
  On the "Scene (General)" tab, you'll find several, but not all, settings that are global to the
  scene, like its scale or music values. You also select the collision model here.

  - Name: Specifies the name of the scene. Currently, that's only used for the scene XML's default
          filename and those of the separate scene and room files created using the "Save Binary"
          menu option.

  - Scale: Sets the scale at which the models (both collision and rooms) will be imported, in case
           you need to change this.

  - BGM: Lets you set which BGM track (in decimal) the scene will play in-game; defaults to Hyrule
         Field.
  
  - Injection Offset: Set the offset (in hexadecimal) at which the scene will be injected to in the
                      ROM when using the "Inject to ROM" function. Make sure you know about what's
                      where in your ROM, because this program doesn't and blindly assumes that you
                      do!

  - Scene Number: Select which scene number your imported scene will overwrite; defaults to 108,
                  which is Sasatest.
  
  - Outdoor Scene (Skybox, Lighting): Checking this will result in the scene being treated as an
                                      outdoor area, so that it will have a skybox, environment-
                                      based lighting and normal day-night cycle. Uncheck this and it
                                      will be treated as an indoor scene, like houses or dungeons,
                                      which don't have skyboxes, time-flow, etc.

  - Collision Model: This is where you select your collision model file. This program expects a
                     model file separate from your rooms' so that you can have things like invisible
                     walls to prevent the player from going out-of-bounds and such. Note that the
                     group names in this file -have to match up- with those in your room files,
                     otherwise collision polygon type settings won't work correctly. A design flaw
                     on my part, so please bear with this kludge for now.

  | Waterboxes |
   This set of controls allows you to add, edit and remove waterboxes from the scene. When adding a
   new waterbox, by default it extends across the whole scene, but you can shrink and reposition it
   any way you like. For the properties (hexadecimal), please consult the z64 wiki and/or just
   experiment.

  | Environment Settings |
   Similar to the waterbox editor, this set of controls allows adding, editing and deleting of
   environment settings, which control lighting, fog and draw distance. Double-click on one of the
   colored boxes to open the color picker dialog, from which you can change the respective color.
   Fog and draw distance are hexadecimal values, which are very sensitive to change, so if you want
   to change them, do so a few bytes at a time.
   
   Please note that, depending on the type of scene (outdoors, indoors), the game expects a certain
   number of environment settings. If you've ever seen the game glitch with weird lighting and/or
   broken Z-buffer ordering, that's the result of invalid environment data.

 c) Transitions & Spawns
 -----------------------
  | Transitions |
   Also similar to the editors for waterboxes and environment settings, this bunch of controls
   allows you to add, edit and delete transition actors like doors or room-changing planes. Of note
   here are the "Front Switch", "Back Switch" and "Cam" values, which are all hexadecimal:

   - Front Switch: Which room this transition actor will load when triggered from the front.

   - Back Switch: Which room the transition actor will load when triggered from its back.

   - Cam: One text box for Front and Back Switch each; this determines how the camera will react
          during the room transition.

   For additional notes on this, see the z64 Wiki.

  | Spawn Points |
   Again, very similar to the previous editors - especially the transition editor -, this allows to
   add and modify spawn points from which Link can spawn. At least one of them is necessary for Link
   to appear in the scene if you want it to be playable. Also, it is advised to use 0000 as the
   actor number and 0FFF as the variable.

 d) Collision & Exits
 --------------------
  This is the place for most of your collision-related needs. Here you can add, edit and delete both
  collision polygon types and exits from the exit list. While the latter is rather straightforward,
  the former is somewhat more complicated.

  | Polygon Types |
   - Raw Data: This is the raw 64-bit polygon type definition. Flags and settings not covered by
               the separate controls below will have to be changed here. Again, please refer to the
               z64 Wiki for more information.

   - Echo Range: Change the echo level; overrides global settings.

   - Environment: Select which environment setup to use; remember that it has to be defined,
                  otherwise you'll encounter the previously mentioned rendering glitches.

   - Terrain Type: Mainly useful to enable or disable the surface steepness, done easily using the
                   "Steep?" checkbox right next to this.

   - Ground Type: Allows selection of the surface's ground type, which determines which sound is
                  played when walked, and if ex. dirt is kicked up when rolled across.

   The following settings define climbability and are more or less self-explanatory, but I'll touch
   upon them lightly anyway:
 
   - Not Climbable: Surface is not climbable, only useful for walls.

   - Ladder-type Climbing: Allows climbing in a straight line, as opposed to the next setting.

   - Whole Surface: Allows climbing across the whole surface, as opposed to the previous setting.

   The next four settings are related to walkability:

   - Default: Non-damaging surface, probably used for most types of ground and walls.

   - Shallow Quicksand: Sand into which Link will sink by a few centimeters.

   - Killing Quicksand: Very deadly quicksand, which will kill Link in a matter of a second or two.

   - Lava: Damaging surface that takes away some health every second.

  | Exit List |
   A very simple editor, which allows adding, editing and deleting of values from the scene's exit
   list. To edit a value, double-click it in the list, then enter your new value into the appearing
   text box. The exit list uses the same values as the game's global exit list, so ex. 00CD for
   Hyrule Field, child, daytime. See the z64 Wiki for the whole list.

 e) Rooms & Group Settings
 -------------------------
  The "Rooms" tab is where you add rooms to your scene, as well as change group settings, like
  translucency or polygon types. The upper list is a list of all rooms in the scene, while the lower
  list shows the groups in the currently selected room.

  Note the "Injection Offset" setting, which determines the ROM offset to which the selected room
  will be imported when using the ROM injection function. This setting is ignored for all but the
  first room if the "Consecutive Room Injection" options is selected in the menu. Yet again, note
  that this -does not check- what data might be overwritten in the ROM during the injection process.

  | Group Settings |
   - Alpha: Determines the opacity/translucency of the selected group. A value of 255 means fully
            opaque, while anything lower makes the converter treat this as a translucent surface.

   - Tint: Double-clicking this box allows you to select a color to tint this group with. White is
           the default color here.

   - Texture Tiling S/T: Allows setting of certain texture properties for the group, S being the X
                         axis, T being Y. Both default to Wrap, which will repeat the texture on the
                         given axis. Clamp will show the texture once, with its outermost line of
                         pixels repeating, while Mirror will do just that.

   - Polygon Type: Select which collision polygon type this group will use. See the section
                   "Collision & Exits" for more about creating and editing them.

   - Backface Culling: Determines if backface culling shall be enabled or disabled for this group.

 f) Objects & Actors
 -------------------
  Finally, this is where you modify the objects and actors of the room currently selected on the
  "Rooms" tab.

  | Objects |
   This group of controls works just like the exit list editor. Add objects, edit them by double-
   clicking on them in the list, delete them by selecting the one to delete and pressing the
   corresponding button.

  | Actors |
   This, in turn, works like the transition and spawn point editors.

  For both editors on this page, remember to check the z64 Wiki for all your variable or object
  number needs.

------
3) FAQ
------
 Here's some questions I suppose you might have, or that have already been asked on various forums,
 and their respective answers:

  Q) It doesn't work! It crashes! It destroys my ROM! etc.
   A) Those are not questions. But anyway, go to either The GCN, z64 or Glitchkill and find the
      active SharpOcarina thread on there, or prepare to send me a private message. Whichever way of
      contacting me you choose, gather as much relevant information about the problems your
      encounter as possible. Say, if the program doesn't start, tell me which version of the .NET
      Framework you have installed, for one. Or if the 3D preview is significantly glitched up, tell
      me what kind of video card you have and if you have the latest drivers installed. If the
      program gives you an error message, give me the full text or a screenshot of the message in
      question.

  Q) How do you do this-or-that?
   A) There's a demonstration scene included with SharpOcarina, which is basically a mini-dungeon
      featuring almost all the things the program supports (thus excluding ex. waterboxes). Load
      this up and look around. Change something, inject it into a ROM, and see what happens. I
      realize that my explanations above might leave a bit to be desired, but this should be a
      reasonable help.

  Q) What games and ROM versions are supported?
   A) For automatic ROM injection, only the OoT Master Quest Debug ROM is supported. If you know
      your way around other versions or Majora's Mask, you -might- be able to manually import the
      separate scene and room files created by the "Save Binary" menu function after some hex editing.

  Q) What texture sizes and formats are supported?
   A) Most textures can be up to 4096 bytes in size once converted. This doesn't mean anything to the
      artist making the textures, so here's some hints about color depths, dimensions, design, etc.
       - Textures with more than 256 colors can be anything up to 64x32 pixels, that means, for example,
         128x16 would also work, while 128x32 would not.
       - Textures with up to 16 colors can be up to 64x64 pixels in size, so 128x32 would work, 128x64
         would not.
       - Try to make use of grayscale textures, which save space and can thus be rather big - with up to
         16 shades of gray, up to 128x64 pixels -, and can still be colorized using the tint options in
         the room's group settings.

  Q) Can I just convert a model into a Display List, without all the scene and room stuff?
   A) Not with this program. The source code is technically capable of that - with minor
      modification, probably - but SharpOcarina is strictly for creating custom maps.

  Q) Do imports done using this program work on a real N64?
   A) To be honest, I have no idea. I don't have the means to run ROMs of any kind on my N64, so I
      cannot test this. If you're able to do that, I'd really like to hear about your results!

  Q) What about [cutscenes; the on-screen map; animated textures; more control over the conversion; etc.]?
   A) There's three possible reasons for a feature being left out of the program:
       1. Not enough is known about the game's inner workings for it to be supported.
       2. It has been left out to simplify the conversion process and not bother the user with too
          many variables.
       3. It is planned for inclusion, but research and/or coding is not done yet.
      The first reason is simple but disappointing. Cutscenes, for example, aren't documented
      enough yet to allow one to create anything elaborate. Modification of existing scenes is
      possible to an extend, but creating new ones from scratch for a custom map is something else.
      The second reason, something being left out for simplicity, is the case with ex. manual
      editing of the scene and room headers. Modifying either in the wrong way can easily break
      them, while most of their information is automatically generated during the conversion
      process anyway. The third and final reason, the feature basically being a work-in-progress, is
      likely the easiest for me to fix, as it's likely a matter of me getting my lazy butt to work.

  Q) Why do you keep refering to the z64 Wiki?
   A) Because it's a great place to get information from, and because I didn't want to just copy its
      explanations.

 Any other questions, comments, etc.? Find me on The GCN, z64 or Glitchkill for that.

-------------------
4) Credits & Thanks
-------------------
 SharpOcarina was written by xdaniel in 2011, using some code by spinout adapted from C and/or
 Python to C#, but in reality is also the product of numerous people who were involved with
 modifying and documenting the Zelda games for the N64 (spinout, MNGoldenEagle, JSA et al.)

 Many thanks to Arcaith for the models in the demonstration project included with SharpOcarina, and
 to Naxylldritt and Zeth for other testing maps used during development.

 Also thanks to everyone who has asked questions and provided ideas during the program's
 development.

 And so that I don't go overboard with this section, I shall cut it short right here.
