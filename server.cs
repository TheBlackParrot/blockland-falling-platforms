exec("./db_functions.cs");
exec("./platform_functions.cs");
exec("./system.cs");
exec("./saving.cs");
exec("./commands.cs");
exec("./shop.cs");

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

$Platforms::Colors = "0 red 3 blue 1 yellow 2 green 8 black 4 white 17 purple 14 orange 27 cyan 24 pink 7 gray 42 brown";
$Platforms::KnownBotUsers = "39943";

function getPlatformColorTypes(%type) {
	switch$(%type) {
		case "numbers":
			for(%i=0;%i<getWordCount($Platforms::Colors);%i+=2) {
				%colors = %colors SPC getWord($Platforms::Colors,%i);
			}
		case "names":
			for(%i=1;%i<getWordCount($Platforms::Colors);%i+=2) {
				%colors = %colors SPC getWord($Platforms::Colors,%i);
			}
	}
	return getSubStr(%colors,1,strLen(%colors));
}

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
			eval("datablock AudioProfile(musicData_FP" @ $Platforms::MusicDataCount @ ") {fileName = \"" @ %file_s @ "\"; description = \"AudioMusicLooping3d\"; preload = 1; uiName = \"" @ strReplace(fileBase(%file_s),"_"," ") @ "\";};");
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
		volume = 0.6;
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

function GameConnection::doBottomStats(%this) {
	cancel(%this.bottomStatLoop);
	%this.bottomStatLoop = %this.schedule(1000,doBottomStats);

	%score = %this.score;
	%totalscore = %this.totalscore;
	%rounds = PlatformAI.rounds;
	%highest[amount] = $Platforms::HighestRound[amount] || 0;
	%highest[name] = $Platforms::HighestRound[name];
	%record = %this.personalRecord || 0;
	if(PlatformAI.roundInitTime == -1) {
		%time = getTimeString(0);
	} else {
		%time = getTimeString(mFloor((getSimTime() - PlatformAI.roundInitTime)/1000));
	}
	if(%highest[name] $= "") {
		%highest[name] = "N/A";
	}

	//%this.bottomPrint("<font:Arial Bold:16>\c3Round:\c6" SPC %rounds @ "  \c3Score:\c6" SPC %score @ "  \c3Total Score:\c6" SPC %totalscore @ "  \c3Longest Survivor:\c6" SPC %highest[name] SPC "[" @ %highest[amount] @ "]  \c3Game Time:\c6" SPC %time,2,1);
	%this.bottomPrint("<font:Arial Bold:14>\c3Round:\c6" SPC %rounds @ "  \c3Time:\c6" SPC %time @ "<just:right>\c3Your Record:\c6" SPC %record SPC " \c3Tickets:\c6" SPC %score @ "  \c3Total Score:\c6" SPC %totalscore @ "<br><just:center><font:Arial Bold:20>\c3Longest Survivor:\c6" SPC %highest[name] SPC "[" @ %highest[amount] @ "]",2,1);
}

function MinigameSO::playSound(%this,%data) {
	for(%i=0;%i<%this.numMembers;%i++) {
		%this.member[%i].play2D(%data);
	}
}

function checkOnDeath() {
	%count = 0;
	%ai = PlatformAI;
	if(!%ai.inProgress) {
		return;
	}
	for(%i=0;%i<ClientGroup.getCount();%i++) {
		%player = ClientGroup.getObject(%i).player;
		if(isObject(%player)) {
			if(%player.inGame) {
				%player[%count] = %player.client;
				%count++;
			}
		}
	}
	if($Platforms::OldCount != %count) {
		messageAll('',"\c4AI: \c6" @ %count SPC "players remain!");
		switch(%count) {
			case 1:
				if(%ai.rounds > 7) {
					%player[0].score += 100;
					%player[0].totalscore += 100;
					messageAll('',"\c4AI: \c6Congratulations to" SPC %player[0].name SPC "for being the last person standing! They receive a 100 ticket bonus!");
					%ai.canBet = 0;
					%ai.doBets(%player[0].player);
				}
			case 2:
				messageAll('',"\c4AI: \c6It's a showdown between" SPC %player[0].name SPC "(" @ %player[0].bl_id @ ") and" SPC %player[1].name SPC "(" @ %player[1].bl_id @ ")! Who will win?");
				messageAll('',"\c4AI: \c6Place your bets! See /help for syntax on /bet, and be sure to use BL_ID's!");
				%ai.canBet = 1;
				%count_b = 0;
				for(%i=0;%i<$DefaultMinigame.numMembers;%i++) {
					%player_b = $DefaultMinigame.member[%i].player;
					if(isObject(%player_b)) {
						if(%player_b.inGame) {
							%ai.pot[%count_b,player] = %player_b;
							%ai.pot[%count_b,amount] = 0;
							%count_b++;
						}
					}
				}
		}
	}
	$Platforms::OldCount = %count;
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
				if(!%player.inGame && %player.client.minigame) {
					%player.inGame = 1;
					messageAll('',"\c3" @ %player.client.name SPC "\c5has joined the game!");
				}
			} else {
				if(!%player.inGame || !%player.client.minigame) {
					%player.kill();
				}
			}
		}

		if(%this.getName() $= "_spawn_teleport") {
			if(%this.canTeleport) {
				%pos = PlatformBricks.getObject(getRandom(0,PlatformBricks.getCount()-1)).brick.getPosition();
				%player.setTransform(getWords(%pos,0,1) SPC getWord(%pos,2) + 5);
				%player.setVelocity("0 0 0");
				%player.setPlayerScale("1 1 1");
				%player.clearTools();
			} else {
				%player.client.centerPrint("\c6This is a teleporter to join the game, however it is not currently active.<br>Wait for the current game to finish first!",3);
			}
		}
	}

	function GameConnection::autoAdminCheck(%this) {
		%this.doBottomStats();
		return parent::autoAdminCheck(%this);
	}

	function ServerLoadSaveFile_End() {
		parent::ServerLoadSaveFile_End();

		if(!$Platforms::HasInit) {
			gatherPlatformBricks();
			gatherProjectileBricks();
			PlatformAI.reset();
		}

		$Platforms::HasInit = 1;
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

	function GameConnection::onDeath(%this,%obj,%killer,%type,%area) {
		if(isObject(%this.player)) {
			if(%this.player.inGame) {
				PlatformAI.activePlayers--;
				if(PlatformAI.rounds > %this.personalRecord) {
					%this.personalRecord = PlatformAI.rounds;
				}
			}
		}
		%this.savePlatformsGame();

		// checking for bot users
		if(PlatformAI.inProgress) {
			%count = 0;
			for(%i=0;%i<ClientGroup.getCount();%i++) {
				%player = ClientGroup.getObject(%i).player;
				if(isObject(%player)) {
					if(%player.inGame) {
						%player[%count] = %player.client;
						%count++;
					}
				}
			}
			if(%count == 4) {
				for(%i=0;%i<3;%i++) {
					if(stripos($Platforms::KnownBotUsers,%player[%i].bl_id) != -1 && !%player[%i].player.isBotKilled) {
						%player[%i].player.kill();
						%player[%i].player.isBotKilled = 1;
						messageAll('',"\c4AI: \c6" @ %player[%i].name SPC "has been known to have used a bot before, preventing them from winning...");
					}
				}
			}
		}

		parent::onDeath(%this,%obj,%killer,%type,%area);
		checkOnDeath();
	}
	function GameConnection::onClientLeaveGame(%this) {
		%r = parent::onClientLeaveGame(%this);
		if(isObject(%this.player)) {
			if(%this.player.inGame) {
				PlatformAI.activePlayers--;
				PlatformAI.players--;
			}
		}
		checkOnDeath();
		return %r;
	}

	// exploits
	function MinigameSO::messageAll(%this) {}
	function MinigameSO::centerPrintAll(%this) {}
	function MinigameSO::bottomPrintAll(%this) {}

	function GunProjectile::onCollision(%this,%obj,%col,%fade,%pos,%normal) {
		if(%col.getClassName() $= "Player") {
			if(%col.inGame) {
				%col.addHealth(-20);
			} else {
				%col.setDamageLevel(0);
				//%obj.player.clearTools();
				%obj.client.player.clearTools();
			}
		}
		return parent::onCollision(%this,%obj,%col,%fade,%pos,%normal);
	}
};
activatePackage(FallingPlatformsPackage);