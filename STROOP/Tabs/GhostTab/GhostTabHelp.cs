using System;

namespace STROOP.Tabs.GhostTab
{
    partial class GhostTab
    {
        private void buttonTutorialRecord_Click(object sender, EventArgs e)
        {
            Forms.InfoForm frm = new Forms.InfoForm();
            frm.Size = new System.Drawing.Size(850, 250);
            frm.SetText("Ghost Help",
                "How to record ghosts",
                @"To record a ghost use the 'recordghost.lua' script.
This script should be located next to your STROOP executable. (You can move it to a different location though.)
When you press 'Start', a new recording will begin at the current frame.
Hitting 'Stop' will save the ghost to 'tmp.ghost' at the location of the script file.
You can then load this file into STROOP to play it back later, or store it somewhere else.
You can also use a File Watcher to automatically load in the last recorded ghost (see ""Using File Watchers"").");
            frm.ShowDialog();
        }

        private void buttonTutorialPlayback_Click(object sender, EventArgs e)
        {
            Forms.InfoForm frm = new Forms.InfoForm();
            frm.Size = new System.Drawing.Size(900, 500);
            frm.SetText("Ghost Help",
                "How to play ghosts back",
                @"To play back a ghost, you must first enable the ghost hack.
If no ghost recording is currently selected, STROOP will instead inject a ghost that will hover around Mario.
This way you can see if the hack is enabled and working correctly.

You can then load up any ghost file or select a previously recorded ghost from the list of ghosts.
Each recording has a 'Playback Start' value that indicates SM64's Global Timer value on the first frame of recorded data.
This value is used to determine when STROOP should inject which ghost frame, so if you're comparing against a ghost recorded from the same savestate as you're currently playing from the ghost should always be synchronized.

If, however, the Global Timer has a different value (i.e. different savestate, different star order, ...), you can resync the playback by modifying the 'Start Playback at Global Timer' value to match the Global Timer of your current run.
For convenience, you can check the game's current Global Timer value in the data view on the right.
Depending on when in the game the ghost recording started (i.e. Star Select screen, First frame of running, ...) you may need to adjust this value until your runs sync up properly.

IMPORTANT: Enabling the ghost hack may break some in-game objects.
Do not start m64's from savestates with the hack already enabled.
Always make sure your runs work from a clean savestate.
");
            frm.ShowDialog();
        }

        private void buttonTutorialNotes_Click(object sender, EventArgs e)
        {
            Forms.InfoForm frm = new Forms.InfoForm();
            frm.Size = new System.Drawing.Size(900, 570);
            frm.SetText("Ghost Help",
                "Notes",
                @"The ghost hack works on the US and JP versions of Super Mario 64 (and therefore also on numerous ROM hacks).
It works best when mupen is set to ""Pure Interpreter"" mode to apply code changes immediately.
If you are not on ""Pure Interpreter"", follow the steps displayed under the ""Enable Ghost Hack"" button exactly and it should still work.

While in most cases the use of the ghost hack is unproblematic, there are several instances in which enabling the ghost hack may alter game behavior. It is therefore recommended that you only use the hack during a recording or playback, but never in a savestate from which an m64 file starts.
Always verify that your runs work without the hack.

Rendering too many ghosts at once can cause a game crash. This number is usually somewhere around 15 to 18, but can be increased by moving the game's Gfx pool. See ""Moving the GFX pool"" for details.

For Usamune ROMs (and perhaps other ROMs as well), you will need to change the ""RAM offset base"" to a higher value (e.g. 8060) before enabling the hack, in order to not conflict with the ROM hack's own memory allocations.
(Reasonable values are between 8040 and 8060 for this hack due to its own memory requirements.)

Disabling the ghost hack via the 'Disable Ghost hack' button may have unintended side effects, including crashing the game.
Try to keep a savestate around that doesn't have the hack enabled instead.

Ghost recordings are purely visual recordings. Some visual effects, such as Mario's upper body tilt, will not perfectly match.
Certain data may look misleading in the map tab. (For instance, the ghost's 'graphics angle' during a sideflip looks inverted.)
The displayed cap state of the ghost is not representative of its actual cap state in the run.
");
            frm.ShowDialog();
        }

        private void buttonHelpGfxPool_Click(object sender, EventArgs e)
        {
            Forms.InfoForm frm = new Forms.InfoForm();
            frm.Size = new System.Drawing.Size(1050, 330);
            frm.SetText("Ghost Help",
                "Moving and expanding the GFX Pool",
                @"You can move the game's GFX pools to a custom location in RAM and assign a custom size to them to allow for more graphics to be rendered.
This can enable you to render a lot of ghosts at once, since the original GFX Pool usually overflows at ~15 to ~18 ghosts.

Unless you are on Pure Interpreter, you will need to savestate and loadstate after moving the GFX pool for the first time.

Be careful not to override any important data such as the ghost hack itself or other hacked data, especially when dealing with ROM hacks.
It is recommended to make a backup savestate before moving the GFX Pool.

If you move the GFX pools more than once and the game has rendered in-between, you may get a warning about overriding non-zero data.
You can usually safely ignore this warning then, although at this point it won't be able to distinguish GFX data from other data."
            );
            frm.ShowDialog();
        }

        private void buttonTutorialFileWatch_Click(object sender, EventArgs e)
        {
            Forms.InfoForm frm = new Forms.InfoForm();
            frm.Size = new System.Drawing.Size(700, 300);
            frm.SetText("Ghost Help",
                "Using the file watch list",
                @"You can let STROOP automatically detect when 'recordghost.lua' saves a new ghost recording.
To do so, click the 'Edit File Watch List' button, then hit 'Add...' and navigate to wherever you have located your 'recordghost.lua' file and select it.

You can add as many instances of the script as you please. The information for paths will be stored in 'Config/Config.xml'
");
            frm.ShowDialog();
        }
    }
}
