function serverCmdHelp(%this) {
	messageClient(%this,'',"\c3/changeMusic \c5[num] \c6-- Changes the music on the server, leave it blank to see a list. \c7[250 tickets]");
	messageClient(%this,'',"\c3/bet \c5[amount] [player] \c6-- Bet on a player.");
	messageClient(%this,'',"\c3/camera \c6-- Watch the game from, well, anywhere! Use the command again to regain control of your player.");
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
	if(%amount <= 0) {
		messageClient(%this,'',"\c6You're trying to bet an invalid amount of tickets!");
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

	if(%ai.pot[0,player] == %selected_player) {
		%ai.pot[0,amount] += %amount;
	}
	if(%ai.pot[1,player] == %selected_player) {
		%ai.pot[1,amount] += %amount;
	}

	messageAll('',"\c3" @ %this.name SPC "\c5has bet\c3" SPC %amount SPC "tickets \c5on\c3" SPC %selected_player.client.name @ "\c5.");
	%this.betContributed[amount] = %amount;
	%this.betContributed[player] = %selected_player;

	%this.score -= %amount;
	%this.savePlatformsGame();
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
		messageClient(%this,'',"\c6The music can only be changed once every 2 minutes. Please wait another\c3" SPC mFloor((getSimTime() - $Platforms::LastMusicChange)/1000) SPC "second(s).");
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