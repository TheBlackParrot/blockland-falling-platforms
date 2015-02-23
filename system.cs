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
	%count = getWordCount(getPlatformColorTypes("numbers"));
	if(%amount > %count) {
		%amount = %count;
	}
	return %amount;
}

function PlatformAI::getDelayReduction(%this) {
	%amount = (%this.rounds-1)*150;
	if(%amount > 3000) {
		%amount = 3000;
	}
	return %amount;
}

function PlatformAI::gameLoop(%this) {
	cancel(%this.gameSchedule);
	%this.gameSchedule = %this.schedule(14500-(%this.getDelayReduction()*2),gameLoop);

	%this.specialRound = 0;

	if(isEventPending(%this.pregameSchedule)) {
		cancel(%this.pregameSchedule);
	}

	%count = 0;
	for(%i=0;%i<ClientGroup.getCount();%i++) {
		%player = ClientGroup.getObject(%i).player;
		if(isObject(%player)) {
			// cheat prevention
			if(getSimTime() - %player.lastTouch >= 30000 && %player.inGame) {
				%player.inGame = 0;
				%player.kill();
			}
			if(%player.inGame) {
				%player[%count] = %player.client;
				%player.client.score += %this.rounds;
				%player.client.totalscore += %this.rounds;
				%player.client.savePlatformsGame();
				%count++;
			}
		}
	}
	if(!%count) {
		%this.stopGame();
		return;
	}
	if(%this.old_count != %count) {
		switch(%count) {
			case 1:
				if(%this.rounds > 7) {
					%player[0].score += 100;
					%player[0].totalscore += 100;
					messageAll('',"\c4AI: \c6Congratulations to" SPC %player[0].name SPC "for being the last person standing! They receive a 100 point bonus!");
				}
			case 2:
				messageAll('',"\c4AI: \c6It's a showdown between" SPC %player[0].name SPC "and" SPC %player[1].name @ "! Who will win?");
		}
	}
	%this.old_count = %count;

	%this.inProgress = 1;

	%color_amount = %this.getColorAmount();
	if(%color_amount != %this.oldColorAmount && %this.oldColorAmount) {
		messageAll('',"\c4AI: \c6Throwing in another color!");
		randomizePlatformBricks(%color_amount);
		%did_shuffle = 1;
	}
	if(%this.rounds % 2 && !%did_shuffle) {
		randomizePlatformBricks(%color_amount);
	}
	%chosen_color = getRandom(0,%color_amount-1);
	if(!getRandom(0,6)) {
		%this.specialRound = 1;
		%this.doSpecialRound(1);
	}
	if(!getRandom(0,6) && %this.rounds >= 15 && !%this.specialRound) {
		%this.inverseFall = 1;
	} else {
		%this.inverseFall = 0;
	}

	//if(!getRandom(0,12)) {
	//	%this.specialRound = 1;
	//	for(%i=0;%i<PlatformBricks.getCount();%i++) {
	//		%brick = 
	//	}
	//}

	if(%this.rounds > $Platforms::HighestRound[amount]) {
		$Platforms::HighestRound[amount] = %this.rounds;
		for(%i=0;%i<ClientGroup.getCount();%i++) {
			%player = ClientGroup.getObject(%i).player;
			if(isObject(%player)) {
				if(%player.inGame) {
					$Platforms::HighestRound[name] = %player.client.name;
				}
			}
		}
	}

	%this.rounds++;
	%this.oldColorAmount = %color_amount;

	if(!%this.specialRound) {
		%this.warnSchedule = schedule(1500-%this.getDelayReduction()/9,0,warnPlatforms,%chosen_color);
		%this.breakSchedule = schedule(5500-%this.getDelayReduction(),0,breakPlatforms,%chosen_color);
	}
}

function PlatformAI::doSpecialRound(%this,%type) {
	for(%i=0;%i<PlatformBricks.getCount();%i++) {
		%brick = PlatformBricks.getObject(%i);
		%brick.selected = 0;
	}
	cancel(%this.warnSchedule);
	cancel(%this.breakSchedule);
	cancel(%this.gameSchedule);
	messageAll("\c4AI: \c6Ooooh, a special round!");

	switch(%type) {
		case 1:
			centerPrintAll("<font:Impact:36>\c6Pay attention! Avoid the selected bricks!",5);
			%amount = getRandom(4,8)*%this.rounds;
			if(%amount > 230) {
				%amount = 230;
			}
			for(%i=0;%i<%amount;%i++) {
				%brick = PlatformBricks.getObject(getRandom(0,PlatformBricks.getCount()-1));
				while(%brick.selected) {
					%brick = PlatformBricks.getObject(getRandom(0,PlatformBricks.getCount()-1));
				}
				%brick.selected = 1;

				%brick = %brick.brick;
				%brick.schedule(%i*50,playSound,brickPlantSound);

				%brick.schedule(%i*50,setColorFX,4);
				%brick.schedule(2000+(%amount*50),fakeKillBrick,getRandom(-20,20) SPC getRandom(-20,20) SPC getRandom(-20,20),6-mFloor(PlatformAI.getDelayReduction()/1000));
				%brick.schedule(2000+(%amount*50),setColorFX,0);
			}
			$DefaultMinigame.schedule(2000+(%amount*50),playSound,"fall" @ getRandom(1,13));
			%this.specialEndSchedule = %this.schedule(8000+(%i*50)+(%amount*50)-PlatformAI.getDelayReduction(),gameLoop);
	}
}

function PlatformAI::stopGame(%this) {
	if(isEventPending(%this.gameSchedule)) {
		cancel(%this.gameSchedule);
		cancel(%this.specialEndSchedule);
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
	%this.hasAwardedBonus = 0;

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

	randomizePlatformBricks(getRandom(2,getWordCount(getPlatformColorTypes("numbers"))));
}