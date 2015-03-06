function serverCmdHelp(%this) {
	messageClient(%this,'',"\c3/changeMusic \c5[num] \c6-- Changes the music on the server, leave it blank to see a list. \c7[250 tickets]");
	messageClient(%this,'',"\c3/bet \c5[amount] [player] \c6-- Bet on a player.");
	messageClient(%this,'',"\c3/camera \c6-- Watch the game from, well, anywhere! Use the command again to regain control of your player.");
	messageClient(%this,'',"\c3/donate \c5[player] [amount] \c6-- Donate some tickets to a player.");
	messageClient(%this,'',"\c3/alive \c6-- See who's still alive at a quick glance.");
	messageClient(%this,'',"\c3/dmroom \c6-- Teleport to the DM Room.");
	messageClient(%this,'',"\c3/shop \c6-- Teleport to the Shop.");
	messageClient(%this,'',"\c3/leaderboard \c6-- Teleport to the Leaderboard.");
	messageClient(%this,'',"\c3/join \c6-- Join the game quickly, although teleporters are radical.");
}

function serverCmdBet(%this,%amount,%target_ask) {
	%ai = PlatformAI;
	%mini = $DefaultMinigame;
	%amount = mFloor(%amount);

	if(%amount $= "" && %target_ask $= "") {
		for(%i=0;%i<$DefaultMinigame.numMembers;%i++) {
			%client = $DefaultMinigame.member[%i];
			if(isObject(%client.player)) {
				if(%client.player.inGame) {
					messageClient(%this,'',"\c3" @ %client.bl_id @ ": \c6" @ %this.name);
				}
			}
		}
		messageClient(%this,'',"\c5Use BL_IDs to bet on players, see the list above.");
		return;
	}

	if(%this.bl_id == %target_ask) {
		messageClient(%this,'',"\c6You cannot bet on yourself.");
		return;
	}
	if(isObject(%this.player)) {
		if(%this.player.inGame) {
			messageClient(%this,'',"\c6You cannot bet while in-game.");
			return;
		}
	}

	if(%this.betContributed[player] !$= "") {
		messageClient(%this,'',"\c6You've already bet\c3" SPC %this.betContributed[amount] SPC "tickets \c6on" SPC %this.betContributed[player].client.name);
		return;
	}

	if(!%ai.canBet) {
		messageClient(%this,'',"\c6Betting is only allowed when the last 2 players are alive and have finished a round only with said players.");
		return;
	}
	if(%amount > %this.score) {
		messageClient(%this,'',"\c6You're trying to bet more tickets than you have!");
		return;
	}
	if(%amount < 100) {
		messageClient(%this,'',"\c6You must bet at least 100 tickets.");
		return;
	}

	%target = findClientByBL_ID(%target_ask);
	if(!isObject(%target)) {
		messageClient(%this,'',"\c6This player doesn't exist. Be sure to use their BL_ID.");
		return;
	}

	%count = 0;
	for(%i=0;%i<$DefaultMinigame.numMembers;%i++) {
		%player = $DefaultMinigame.member[%i].player;
		if(isObject(%player)) {
			if(%player.inGame) {
				%player[%count] = %player;
				if(%target == %player.client) {
					%selected_player = %player;
				}
				%count++;
			}
		}
	}

	if(!%selected_player) {
		messageClient(%this,'',"\c6This player isn't one of the last two survivors.");
		return;
	}

	for(%i=0;%i<2;%i++) {
		if(%ai.pot[%i,player] == %selected_player) {
			%percent = (%this.betContributed[amount]/%ai.pot[%i,amount])*100;
			%limit = mCeil(%ai.pot[%i,amount]*(%percent/100));
			if(%amount >= %limit) {
				%ai.pot[%i,amount] += %amount;
			} else {
				messageClient(%this,'',"\c6The current bet limit for this player is\c3" SPC %limit SPC "tickets.");
				return;
			}
		}
	}

	messageAll('',"\c3" @ %this.name SPC "\c5has bet\c3" SPC %amount SPC "tickets \c5on\c3" SPC %selected_player.client.name @ "\c5.");
	%this.betContributed[amount] = %amount;
	%this.betContributed[player] = %selected_player;

	%this.score -= %amount;
	%this.savePlatformsGame();

	//PlatformAI.checkCurrentBets(%this);
}

function serverCmdChangeMusic(%this,%which) {
	if(%which $= "" || %which <= 0 || %which > $Platforms::MusicDataCount) {
		for(%i=0;%i<$Platforms::MusicDataCount;%i++) {
			%music_obj = "musicData_FP" @ %i;
			messageClient(%this,'',"\c3" @ %i+1 @ ".\c6" SPC %music_obj.uiName);
		}
		return;
	}
	if(%this.score < 250) {
		messageClient(%this,'',"\c6You need at least 250 tickets to change the music.");
		return;
	}
	if(getSimTime() - $Platforms::LastMusicChange < 120000) {
		messageClient(%this,'',"\c6The music can only be changed once every 2 minutes. Please wait another\c3" SPC mFloor((($Platforms::LastMusicChange+120000)-getSimTime())/1000) SPC "second(s).");
		return;
	}

	// index starts at 0, we show the client starting at 1
	%which -= 1;

	%this.score -= 250;
	%music_obj = "musicData_FP" @ %which;
	$Platforms::LastMusicChange = getSimTime();
	messageAll('',"\c3" @ %this.name SPC "\c6changed the music to\c3" SPC %music_obj.uiName);

	// i would just set the profile and update it, but that's not working
	// i bet this is why the default music bricks have a new AudioEmitter everytime it's changed
	if(isObject($Platforms::Music)) {
		$Platforms::Music.delete();
	}
	$Platforms::Music = new AudioEmitter() {
		is3D = 0;
		profile = "musicData_FP" @ %which;
		referenceDistance = 999999;
		maxDistance = 999999;
		volume = 0.6;
		position = $loadOffset;
	};
}

function serverCmdCamera(%this) {
	if(%this.getControlObject().getClassName() $= "Player") {
		%camera = %this.Camera;
		%camera.setFlyMode();
		%camera.mode = "Observer";
		%this.setControlObject(%camera);
		return;
	}
	if(%this.getControlObject().getClassName() $= "Camera") {
		if(!isObject(%this.player)) {
			%this.spawnPlayer();
		}
		%this.player.instantRespawn();
	}
}

function serverCmdDonate(%this,%target,%amount) {
	if(%amount > %this.score) {
		messageClient(%this,'',"\c6You can't donate more tickets than you have!");
		return;
	}
	if(%amount <= 0) {
		messageClient(%this,'',"\c6You must donate at least something.");
		return;
	}
	%target = findClientByName(%target);
	if(!isObject(%target)) {
		messageClient(%this,'',"\c3" @ %target SPC "\c6doesn't exist!");
		return;
	}

	%target.score += %amount;
	messageClient(%target,'',"\c3" @ %this.name SPC "\c6has donated\c3" SPC %amount SPC "tickets \c6to you");
	%this.score -= %amount;
	messageClient(%this,'',"\c6You have donated\c3" SPC %amount SPC "tickets \c6to\c3" SPC %target.name);

	%this.savePlatformsGame();
	%target.savePlatformsGame();
}

function serverCmdDMRoom(%this) {
	if(!isObject(%this.player)) {
		return;
	}
	if(%this.player.inGame) {
		return;
	}
	%brick = "_dm_room_outside";
	%this.player.setTransform(%brick.getPosition());
}
function serverCmdShop(%this) {
	if(!isObject(%this.player)) {
		return;
	}
	if(%this.player.inGame) {
		return;
	}
	%brick = "_hat_shop_spot";
	%this.player.setTransform(%brick.getPosition());
}
function serverCmdLeaderboard(%this) {
	if(!isObject(%this.player)) {
		return;
	}
	if(%this.player.inGame) {
		return;
	}
	%brick = "_leaderboard_spot";
	%this.player.setTransform(%brick.getPosition());
	for(%i=1;%i<=26;%i++) {
		if(%this.name $= $Platforms::Leaderboard[%i,name]) {
			messageClient(%this,'',"\c6You are in\c3" SPC getPositionString(%i) SPC "place.");
			break;
		}
	}
}
function serverCmdAlive(%this) {
	if(getSimTime() - %this.lastalivecmd <= 1000) {
		return;
	}
	%this.lastalivecmd = getSimTime();

	for(%i=0;%i<$DefaultMinigame.numMembers;%i++) {
		%client = $DefaultMinigame.member[%i];
		if(isObject(%client.player)) {
			if(%client.player.inGame) {
				%count++;
				messageClient(%this,'',"\c3" @ %count @ ".\c6" SPC %client.name);
			}
		}
	}
}
function serverCmdJoin(%this) {
	if(!isObject(%this.player)) {
		return;
	}
	if(%this.player.inGame) {
		return;
	}
	if(PlatformAI.inProgress) {
		return;
	}
	%pos = PlatformBricks.getObject(getRandom(0,PlatformBricks.getCount()-1)).brick.getPosition();
	%this.player.setTransform(getWords(%pos,0,1) SPC getWord(%pos,2) + 5);
	%this.player.setVelocity("0 0 0");
	%this.player.setPlayerScale("1 1 1");
	%this.player.clearTools();
	%this.player.changeDatablock(PlayerPlatforms);
}