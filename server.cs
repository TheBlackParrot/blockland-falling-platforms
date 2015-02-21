exec("./db_functions.cs");
exec("./platform_functions.cs");
exec("./system.cs");

datablock AudioProfile(fall1)
{
	filename = "./sounds/fall1.wav";
	description = AudioClosest3d;
	preload = true;
};
datablock AudioProfile(fall2:fall1) { filename = "./sounds/fall2.wav"; };
datablock AudioProfile(fall3:fall1) { filename = "./sounds/fall3.wav"; };
datablock AudioProfile(fall4:fall1) { filename = "./sounds/fall4.wav"; };
datablock AudioProfile(fall5:fall1) { filename = "./sounds/fall5.wav"; };
datablock AudioProfile(fall6:fall1) { filename = "./sounds/fall6.wav"; };
datablock AudioProfile(fall7:fall1) { filename = "./sounds/fall7.wav"; };
datablock AudioProfile(fall8:fall1) { filename = "./sounds/fall8.wav"; };
datablock AudioProfile(fall9:fall1) { filename = "./sounds/fall9.wav"; };
datablock AudioProfile(fall10:fall1) { filename = "./sounds/fall10.wav"; };
datablock AudioProfile(fall11:fall1) { filename = "./sounds/fall11.wav"; };
datablock AudioProfile(fall12:fall1) { filename = "./sounds/fall12.wav"; };
datablock AudioProfile(fall13:fall1) { filename = "./sounds/fall13.wav"; };
datablock AudioProfile(color_notif:fall1) { filename = "./sounds/color.wav"; };

AudioMusicLooping3d.is3d = 0;
AudioMusicLooping3d.referenceDistance = 999999;
AudioMusicLooping3d.maxDistance = 999999;

function initFPMusic() {
	%path = "Add-Ons/Music/";
	%file_s = findFirstFile(%path @ "*.ogg");
	$Platforms::MusicDataCount = 0;
	echo(%file_s);

	while(%file_s !$= "") {
		%str = "musicData_" @ fileBase(%file_s);
		// get rid of default music
		%blacklist = "After_School_Special.ogg Ambient_Deep.ogg Bass_1.ogg Bass_2.ogg Bass_3.ogg Creepy.ogg Distort.ogg Drums.ogg Factory.ogg Icy.ogg Jungle.ogg Paprika_-_Byakko_no.ogg Peaceful.ogg Piano_Bass.ogg Rock.ogg Stress_.ogg Vartan_-_Death.ogg";
		if(stripos(%blacklist,fileName(%file_s)) == -1) {
			// ugh, i hate using eval
			eval("datablock AudioProfile(musicData_FP" @ $Platforms::MusicDataCount @ ") {fileName = \"" @ %file_s @ "\"; description = \"AudioMusicLooping3d\"; preload = 1; uiName = \"" @ %str @ "\";};");
			$Platforms::MusicData[$Platforms::MusicDataCount] = %str;
			$Platforms::MusicDataCount++;
		} else {
			warn("SKIPPED" SPC fileName(%file_s) @ ", considering a default loop.");
		}
		%file_s = findNextFile(%path @ "*.ogg");
	}

	if(isObject($Platforms::Music)) {
		$Platforms::Music.delete();
	}
	$Platforms::Music = new AudioEmitter() {
		is3D = 0;
		profile = "musicData_FP" @ getRandom(0,$Platforms::MusicDataCount);
		referenceDistance = 999999;
		maxDistance = 999999;
		volume = 0.5;
	};
}
initFPMusic();

function RGBToHex(%rgb) {
	%rgb = getWords(%rgb,0,2);
	for(%i=0;%i<getWordCount(%rgb);%i++) {
		%dec = mFloor(getWord(%rgb,%i)*255);
		%str = "0123456789ABCDEF";
		%hex = "";

		while(%dec != 0) {
			%hexn = %dec % 16;
			%dec = mFloor(%dec / 16);
			%hex = getSubStr(%str,%hexn,1) @ %hex;    
		}

		if(strLen(%hex) == 1)
			%hex = "0" @ %hex;
		if(!strLen(%hex))
			%hex = "00";

		%hexstr = %hexstr @ %hex;
	}

	if(%hexstr $= "") {
		%hexstr = "FF00FF";
	}
	return %hexstr;
}

package FallingPlatformsPackage {
	function fxDTSBrick::onAdd(%this) {
		%this.enableTouch = 1;
		return parent::onAdd(%this);
	}

	function fxDTSBrick::onPlayerTouch(%this,%player) {
		parent::onPlayerTouch(%this,%player);
		
		// cheat prevention
		if(%this.getName() $= "_falling_plate") {
			%player.lastTouch = getSimTime();
			if(!PlatformAI.inProgress) {
				if(!%player.inGame) {
					%player.inGame = 1;
					messageAll('',"\c3" @ %player.client.name SPC "\c6has joined the game!");
				}
			}
		}

		if(%this.getName() $= "_spawn_teleport") {
			echo("TELEPORT");
			if(%this.canTeleport) {
				%pos = PlatformBricks.getObject(getRandom(0,PlatformBricks.getCount()-1)).brick.getPosition();
				%player.setTransform(getWords(%pos,0,1) SPC getWord(%pos,2) + 5);
				%player.setVelocity("0 0 0");
			}
		}
	}

	function ServerLoadSaveFile_End() {
		parent::ServerLoadSaveFile_End();

		gatherPlatformBricks();
		PlatformAI.reset();
	}

	function onServerDestroyed() {
		%ai = PlatformAI;
		cancel(%ai.readySchedule);
		cancel(%ai.breakSchedule);
		cancel(%ai.warnSchedule);
		cancel(%ai.gameSchedule);
		cancel(%ai.resetSchedule);
		cancel(%ai.startSchedule);
		%ai.delete();
		deleteVariables("$Platforms::*");

		return parent::onServerDestroyed();
	}
};
activatePackage(FallingPlatformsPackage);