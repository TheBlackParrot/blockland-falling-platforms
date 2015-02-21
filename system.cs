if(!isObject(PlatformAI)) {
	new ScriptObject(PlatformAI) {
		initTime = getSimTime();
		rounds = 1;
		roundInitTime = -1;
		inProgress = 0;
		oldColorAmount = 0;
	};
}

function PlatformAI::getColorAmount(%this) {
	%amount = mCeil(%this.rounds/5)+1;
	if(%amount > 8) {
		%amount = 8;
	}
	return %amount;
}

function PlatformAI::getDelayReduction(%this) {
	%amount = (%this.rounds-1)*75;
	if(%amount > 4000) {
		%amount = 4000;
	}
	return %amount;
}

function PlatformAI::gameLoop(%this) {
	cancel(%this.gameSchedule);
	%this.gameSchedule = %this.schedule(15000-%this.getDelayReduction()*1.6,gameLoop);

	if(isEventPending(%this.pregameSchedule)) {
		cancel(%this.pregameSchedule);
	}

	for(%i=0;%i<ClientGroup.getCount();%i++) {
		%player = ClientGroup.getObject(%i).player;
		if(isObject(%player)) {
			// cheat prevention
			if(getSimTime() - %player.lastTouch >= 30000 && %player.inGame) {
				%player.inGame = 0;
				%player.kill();
			}
			if(%player.inGame) {
				%count++;
			}
		}
	}
	if(!%count) {
		%this.stopGame();
		return;
	}

	%this.inProgress = 1;

	%color_amount = %this.getColorAmount();
	if(%color_amount != %this.oldColorAmount && %this.oldColorAmount) {
		messageAll('',"\c4AI: \c6Throwing in another color!");
	}
	randomizePlatformBricks(%color_amount);
	%chosen_color = getRandom(0,%color_amount-1);

	%this.warnSchedule = schedule(1000-%this.getDelayReduction()/9,0,warnPlatforms,%chosen_color);
	%this.breakSchedule = schedule(5000-%this.getDelayReduction(),0,breakPlatforms,%chosen_color);

	%this.rounds++;
	%this.oldColorAmount = %color_amount;
}

function PlatformAI::stopGame(%this) {
	if(isEventPending(%this.gameSchedule)) {
		cancel(%this.gameSchedule);
		// make sure all players still in-game are kicked out
		messageAll('',"\c4AI: \c6Good game!");
		%this.resetSchedule = %this.schedule(2000,reset);
	} else {
		messageAll('',"\c4AI: \c6There's not a game in progress. This appears to be a bug...");
	}
}

function PlatformAI::reset(%this) {
	%this.rounds = 1;
	%this.roundInitTime = -1;
	%this.inProgress = 0;
	%this.oldColorAmount = 0;

	for(%i=0;%i<ClientGroup.getCount();%i++) {
		%player = ClientGroup.getObject(%i).player;
		if(isObject(%player)) {
			if(%player.inGame) {
				%player.client.instantRespawn();
			}
		}
	}

	for(%i=0;%i<BrickGroup_888888.getCount();%i++) {
		%brick = BrickGroup_888888.getObject(%i);
		if(%brick.getName() $= "_spawn_teleport") {
			%brick.canTeleport = 1;
			%brick.setColor(13);
			%brick.setColorFX(1);
			%brick.setEmitter(LaserEmitterA);
		}
	}

	messageAll('',"\c4AI: \c6A new game is about to start! Use the corner teleporters to join in.");
	%this.pregameLoop();
	%this.readySchedule = %this.schedule(30000,readyGame);
}

function PlatformAI::readyGame(%this) {
	for(%i=0;%i<ClientGroup.getCount();%i++) {
		%player = ClientGroup.getObject(%i).player;
		if(isObject(%player)) {
			if(%player.inGame) {
				%count++;
			}
		}
	}
	if(!%count) {
		messageAll('',"\c4AI: \c6Aw, no one has joined yet. I'll wait a little bit longer.");
		%this.readySchedule = %this.schedule(30000,readyGame);
		return;
	}
	%this.roundInitTime = getSimTime();
	%this.inProgress = 1;
	for(%i=0;%i<BrickGroup_888888.getCount();%i++) {
		%brick = BrickGroup_888888.getObject(%i);
		if(%brick.getName() $= "_spawn_teleport") {
			%brick.canTeleport = 0;
			%brick.setColor(59);
			%brick.setColorFX(0);
			%brick.setEmitter(0);
		}
	}
	messageAll('',"\c4AI: \c6Let's begin!");
	%this.startSchedule = %this.schedule(5000,gameLoop);
}

function PlatformAI::pregameLoop(%this) {
	cancel(%this.pregameSchedule);
	%this.pregameSchedule = %this.schedule(2000,pregameLoop);

	randomizePlatformBricks(getRandom(2,8));
}