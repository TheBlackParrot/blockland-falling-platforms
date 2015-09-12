$Platforms::LayoutDir = "Add-Ons/Gamemode_Falling_Platforms/layouts/";

datablock PlayerData(PlayerPlatforms : PlayerStandardArmor) {
	airControl = 0.2;
	jumpForce = 1280;
	maxForwardSpeed = 10;
	maxSideSpeed = 10;
	maxBackwardSpeed = 10;
	runForce = 8000;
	groundImpactShakeAmp = "0.5 0.5 0.5";
	groundImpactMinSpeed = 13;
	minJetEnergy = 0;
	jetEnergyDrain = 0;
	canJet = 0;
	uiName = "Platforms Player";
	showEnergyBar = false;
};

exec("./db_functions.cs");
exec("./layouts.cs");
exec("./platform_functions.cs");
exec("./system.cs");
exec("./saving.cs");
exec("./commands.cs");
//exec("./shop.cs"); (deprecated)
exec("./support.cs");
exec("./leaderboard.cs");
loadLeaderboard();
exec("./events.cs");
exec("./achievements.cs");

// might throw off bots initially
if(!$Platforms::ChangedLoadOffset) {
	$loadOffset = getRandom(-1000,1000) SPC getRandom(-1000,1000) SPC 0;
	// putting this here, don't mind meee
	PlatformAI.practiceLoop();
}
$Platforms::ChangedLoadOffset = 1;

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
PushbroomProjectile.impactImpulse = 2600;

$Platforms::Colors = "0 red 3 blue 1 yellow 2 green 8 black 4 white 17 purple 14 orange 28 cyan 24 pink 7 gray 43 brown 10 teal 26 lavender 38 tan 12 lime";

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
		position = $loadOffset;
	};
}
initFPMusic();

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
	%winloss = (%this.wins || 0) @ ":" @ (%this.losses || 0);

	//%this.bottomPrint("<font:Arial Bold:16>\c3Round:\c6" SPC %rounds @ "  \c3Score:\c6" SPC %score @ "  \c3Total Score:\c6" SPC %totalscore @ "  \c3Longest Survivor:\c6" SPC %highest[name] SPC "[" @ %highest[amount] @ "]  \c3Game Time:\c6" SPC %time,2,1);
	%this.bottomPrint("<font:Arial Bold:14>\c3Round:\c6" SPC %rounds @ "  \c3Time:\c6" SPC %time @ "<just:right>\c3W/L:\c6" SPC %winloss @ "  \c3Your Record:\c6" SPC %record SPC " \c3Tickets:\c6" SPC %score @ "  \c3Total Score:\c6" SPC %totalscore @ "<br><just:center><font:Arial Bold:20>\c3Longest Survivor:\c6" SPC %highest[name] SPC "[" @ %highest[amount] @ "]",2,1);
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
				if(%ai.rounds > 7 && %ai.players > 2) {
					%player[0].score += 1000+mPow(PlatformAI.players,3);
					%player[0].totalscore += 100+(PlatformAI.players*2);
					messageAll('',"\c4AI: \c6Congratulations to" SPC %player[0].name SPC "for being the last person standing! They receive a" SPC 1000+mPow(PlatformAI.players,3) SPC "ticket bonus!");
					%ai.canBet = 0;
					%ai.doBets(%player[0].player);
				}
			case 2:
				messageAll('',"\c4AI: \c6It's a showdown between" SPC %player[0].name SPC "(" @ %player[0].bl_id @ ") and" SPC %player[1].name SPC "(" @ %player[1].bl_id @ ")! Who will win?");
				messageAll('',"\c4AI: \c6Place your bets! See /help for syntax on /bet, and be sure to use BL_ID's! 500 tickets has been placed on the house. \c3You have 17 seconds to place a bet.");
				%ai.canBet = 1;
				%ai.betsStartedAt = getSimTime();
				%count_b = 0;
				for(%i=0;%i<$DefaultMinigame.numMembers;%i++) {
					%player_b = $DefaultMinigame.member[%i].player;
					if(isObject(%player_b)) {
						if(%player_b.inGame) {
							%ai.pot[%count_b,player] = %player_b;
							%ai.pot[%count_b,amount] = 500;
							%count_b++;
						}
					}
				}
		}
	}
	$Platforms::OldCount = %count;
}

function Player::getLookingAt(%this,%distance)
{
	if(!%this.inGame) {
		return;
	}

	if(!%distance) {
		%distance = 5;
	}

	%eye = vectorScale(%this.getEyeVector(),%distance);
	%pos = %this.getEyePoint();
	%mask = $TypeMasks::FxBrickObjectType;
	%hit = firstWord(containerRaycast(%pos, vectorAdd(%pos, %eye), %mask, %this));
		
	if(!isObject(%hit)) {
		return;
	}
		
	if(%hit.getClassName() $= "fxDTSBrick") {
		return %hit;
	}
}

function TipBotLoop(%line) {
	if($Platforms::Tip[0] $= "") {
		gatherTipBotLines();
	}
	if($Platforms::Tip[%line] $= "") {
		%line = 0;
	}

	cancel($Platforms::TipBotSched);
	$Platforms::TipBotSched = schedule(45000,0,TipBotLoop,%line+1);

	%str = strReplace($Platforms::Tip[%line],"%%LAYOUTS",PlatformLayouts.getCount());

	messageAll('',"\c1Tip:\c6" SPC %str);
}
if(!$Platforms::TipBotSched) {
	TipBotLoop(0);
}

package FallingPlatformsPackage {
	function fxDTSBrick::onAdd(%this) {
		%this.enableTouch = 1;
		return parent::onAdd(%this);
	}

	function armor::onTrigger(%db,%obj,%slot,%val) {
		if(%obj.getClassName() $= "Player") {
			if(!%obj.inGame || !%obj.canSpleefPlates) {
				return Parent::onTrigger(%db,%obj,%slot,%val);
			}
			if(%val == 1 && !%slot) {
				%brick = %obj.getLookingAt();
				if(isObject(%brick)) {
					if(!isEventPending(%brick.breakBrickSched[1])) {
						%brick.setColorFX(3);
						%brick.playSound(brickPlantSound);
						%brick.breakBrickSched[1] = %brick.schedule(500,fakeKillBrick,"0 0 0",3);
						%brick.breakBrickSched[2] = %brick.schedule(500,playSound,brickBreakSound);
						%brick.breakBrickSched[3] = %brick.schedule(600,setColorFX,0);
					}
				}
			}
		}

		return Parent::onTrigger(%db,%obj,%slot,%val);
	}

	function fxDTSBrick::onPlayerTouch(%this,%player) {
		parent::onPlayerTouch(%this,%player);

		if(%this.getName() $= "_dm_room_floor") {
			%player.addNewItem("Sword");
			%player.addNewItem("Gun");
			%player.canBeKilled = 1;
		}
		if(%this.getName() $= "_dm_room_outside") {
			%player.canBeKilled = 0;
		}
		if(%this.getName() $= "_practice_plate") {
			%player.changeDatablock(PlayerPlatforms);
		}

		if(%this.getName() $= "_spawn_teleport") {
			if(%this.canTeleport) {
				%pos = PlatformBricks.getObject(getRandom(0,PlatformBricks.getCount()-1)).brick.getPosition();
				%player.setTransform(getWords(%pos,0,1) SPC getWord(%pos,2) + 5);
				%player.setVelocity("0 0 0");
				%player.setPlayerScale("1 1 1");
				%player.clearTools();
				%player.changeDatablock(PlayerPlatforms);
			} else {
				%player.client.centerPrint("\c6This is a teleporter to join the game, however it is not currently active.<br>Wait for the current game to finish first!",3);
			}
		}
		
		if(%this.getName() $= "_falling_plate") {
			%player.lastTouch = getSimTime();
			if(!PlatformAI.inProgress) {
				if(!%player.inGame && isObject(%player.client.minigame)) {
					%player.inGame = 1;
					messageAll('',"\c3" @ %player.client.name SPC "\c5has joined the game!");
					PlatformAI.players++;
					PlatformAI.activePlayers++;
				}
			} else {
				if(!%player.inGame) {
					%player.kill();
				}
				if(%player.canBreakPlates && %player.inGame) {
					if(!isEventPending(%this.breakBrickSched[1])) {
						%this.setColorFX(3);
						%this.breakBrickSched[1] = %this.schedule(200,fakeKillBrick,"0 0 0",3);
						%this.breakBrickSched[2] = %this.schedule(200,playSound,brickBreakSound);
						%this.breakBrickSched[3] = %this.schedule(300,setColorFX,0);
					}
				}
			}
		}
	}

	function GameConnection::autoAdminCheck(%this) {
		%this.doBottomStats();
		if(%this.original_prefix $= "") {
			%this.original_prefix = %this.clanPrefix;
		}

		return parent::autoAdminCheck(%this);
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
		// prevents non-dedicated servers from looping a random sound on gamemode restart
		// A+ handling there, torque
		if(isObject($Platforms::Music)) {
			$Platforms::Music.delete();
		}
		if(isObject(BrickGroup_Platforms)) {
			BrickGroup_Platforms.deleteAll();
			BrickGroup_Platforms.delete();
		}
		deleteVariables("$Platforms::*");
		$loadOffset = "0 0 0";

		return parent::onServerDestroyed();
	}

	function GameConnection::onDeath(%this,%obj,%killer,%type,%area) {
		if(isObject(%this.player)) {
			if(%this.player.inGame) {
				PlatformAI.activePlayers--;
				if(!PlatformAI.inProgress) {
					PlatformAI.players--;
					return parent::onDeath(%this,%obj,%killer,%type,%area);
				}
				if(PlatformAI.rounds > %this.personalRecord) {
					%this.personalRecord = PlatformAI.rounds;
				}
				if(PlatformAI.players > 1) {
					if(PlatformAI.activePlayers == 3) { %this.awardAchievement("A03"); }
					if(PlatformAI.activePlayers == 2) { %this.awardAchievement("A02"); }

					if(PlatformAI.activePlayers < 2) {
						%this.awardAchievement("A01");

						%hat = HatMod_getRandomHat();
						HatMod_addHat(%this, %this.bl_id, %hat, 1);
						messageClient(%this, '', "\c6You have received the\c3" SPC %hat SPC "\c6hat for winning.");
						
						%this.wins++;
					} else {
						%this.losses++;
					}
				}
				
				if(PlatformAI.rounds < 5) { %this.awardAchievement("A04"); }
				if(PlatformAI.rounds >= 60) { %this.awardAchievement("A05"); }

				if(isObject(%obj)) {
					switch$(%obj.getDatablock().getName()) {
						case "gravityRocketProjectile":
							%this.awardAchievement("A06");
						case "GunProjectile":
							%this.awardAchievement("A0C");
					}
				}
			}
		}
		%this.savePlatformsGame();

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
	function serverCmdDropTool(%this) {}

	function GunProjectile::onCollision(%this,%obj,%col,%fade,%pos,%normal) {
		if(%col.getClassName() $= "Player") {
			if(%col.inGame) {
				%col.addHealth(-20);
			} else {
				if(!%col.canBeKilled) {
					%col.setDamageLevel(0);
					%obj.client.player.clearTools();
				}
			}
		}
		return parent::onCollision(%this,%obj,%col,%fade,%pos,%normal);
	}

	function PlayerPlatforms::onEnterLiquid(%data,%obj,%coverage,%type) {
		if(isObject(%obj.client.minigame) && %obj.inGame) {
			if(!PlatformAI.inProgress) {
				%pos = PlatformBricks.getObject(getRandom(0,PlatformBricks.getCount()-1)).brick.getPosition();
				%obj.setTransform(getWords(%pos,0,1) SPC getWord(%pos,2)+5);
				%obj.setVelocity("0 0 0");
				%obj.setDamageLevel(0);
			}
		}
		return parent::onEnterLiquid(%data,%obj,%coverage,%type);
	}

	function serverCmdMessageSent(%client, %msg) {
		%row = PlatformsLeaderboard.getRowNumByID(%client.bl_id);
		if(%row == -1) {
			return parent::serverCmdMessageSent(%client, %msg);
		}

		%client.clanPrefix = "\c7[\c5" @ getPositionString(%row+1) @ "\c7]" SPC %client.original_prefix;
		return parent::serverCmdMessageSent(%client, %msg);
	}
};
activatePackage(FallingPlatformsPackage);
