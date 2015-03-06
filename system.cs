if(!isObject(PlatformAI)) {
	new ScriptObject(PlatformAI) {
		initTime = getSimTime();
		rounds = 0;
		roundInitTime = -1;
		inProgress = 0;
		oldColorAmount = 0;
		gamesPlayed = 0;
	};
}

function PlatformAI::getColorAmount(%this) {
	%amount = mCeil(%this.rounds/3)+1;
	%count = getWordCount(getPlatformColorTypes("numbers"));
	if(%amount > %count) {
		%amount = %count;
	}
	return %amount;
}

function PlatformAI::getDelayReduction(%this) {
	%amount = (%this.rounds-1)*150;
	// really having to compensate for recent player changes
	// holy crap
	if(%amount > 3450) {
		%amount = 3450;
	}
	return %amount;
}

function PlatformAI::gameLoop(%this) {
	cancel(%this.gameSchedule);
	%this.gameSchedule = %this.schedule(14200-(%this.getDelayReduction()*2),gameLoop);

	%this.specialRound = 0;
	%this.rounds++;

	if(isEventPending(%this.pregameSchedule)) {
		cancel(%this.pregameSchedule);
	}

	if(%this.players >= 10 && %this.activePlayers <= 1) {
		for(%i=0;%i<$DefaultMinigame.numMembers;%i++) {
			%player = $DefaultMinigame.member[%i].player;
			if(isObject(%player)) {
				if(%player.inGame) {
					messageClient(%player.client,'',"\c6You were killed to keep the game moving. You can try to beat the record if 10 or less players are playing.");
					%this.stopGame();
					if(PlatformAI.rounds > %player.client.personalRecord) {
						%player.client.personalRecord = PlatformAI.rounds;
					}
					%player.kill();
					return;
				}
			}
		}
	}

	%count = 0;
	for(%i=0;%i<ClientGroup.getCount();%i++) {
		%player = ClientGroup.getObject(%i).player;
		if(isObject(%player)) {
			// cheat prevention
			if(getSimTime() - %player.lastTouch >= 30000 && %player.inGame) {
				%player.inGame = 0;
				%player.kill();
				messageClient(%player.client,'',"\c6You were killed via cheat prevention. Please make sure you touch a plate at least every 30 seconds.");
			}
			if(%player.inGame) {
				%player[%count] = %player.client;
				%player.client.score += %this.rounds * mPow((%this.players - (%this.activePlayers-1)),2);
				%player.client.totalscore += %this.rounds;
				%player.client.savePlatformsGame();
				%count++;
				if(PlatformAI.rounds > %player.client.personalRecord) {
					%player.client.personalRecord = PlatformAI.rounds;
				}
			}
		}
	}
	if(!%count) {
		%this.stopGame();
		return;
	}
	%this.old_count = %count;

	%this.inProgress = 1;

	%color_amount = %this.getColorAmount();
	%delay_reduction = 1000;
	if(%color_amount != %this.oldColorAmount && %this.oldColorAmount) {
		messageAll('',"\c4AI: \c6Throwing in another color!");
		randomizePlatformBricks(%color_amount);
		%did_shuffle = 1;
		%delay_reduction = 0;
	}
	if(%this.rounds % 2 && !%did_shuffle) {
		randomizePlatformBricks(%color_amount);
		%delay_reduction = 0;
	}
	%chosen_color = getRandom(0,%color_amount-1);
	if(!getRandom(0,6) && %this.rounds >= 7) {
		%this.specialRound = 1;
		if(%this.activePlayers > 1) {
			%rand = getRandom(1,6);
			%max = 6;
			//%this.doSpecialRound(3);
		} else {
			%rand = getRandom(1,4);
			%max = 4;
		}
		if(!ProjectileBricks.getCount()) {
			while(%rand == 2) {
				%rand = getRandom(1,%max);
			}
		}
		if(%this.getColorAmount() > 6) {
			while(%rand == 3) {
				%rand = getRandom(1,%max);
			}
		}
		%this.doSpecialRound(%rand);
	}
	if(!getRandom(0,6) && %this.getColorAmount() >= 3 && %this.getColorAmount() <= 6 && !%this.specialRound) {
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

	%this.oldColorAmount = %color_amount;

	if(!%this.specialRound) {
		%this.warnSchedule = schedule(1500-%this.getDelayReduction()/9-%delay_reduction,0,warnPlatforms,%chosen_color);
		%this.breakSchedule = schedule(5500-%this.getDelayReduction()-%delay_reduction,0,breakPlatforms,%chosen_color);
	}
}

function PlatformAI::doSpecialRound(%this,%type) {
	for(%i=0;%i<PlatformBricks.getCount();%i++) {
		%brick = PlatformBricks.getObject(%i);
		%brick.selected = 0;
		%brick.brick.setColorFX(4);
		%brick.brick.setColorFX(0);
	}
	cancel(%this.warnSchedule);
	cancel(%this.breakSchedule);
	cancel(%this.gameSchedule);
	messageAll('',"\c4AI: \c6Ooooh, a special round!");

	switch(%type) {
		case 1:
			centerPrintAll("<font:Impact:36>\c6Pay attention! Avoid the selected bricks!",5);
			%amount = getRandom(mFloor(PlatformBricks.getCount()/100)*2,mFloor(PlatformBricks.getCount()/100)*4)*%this.rounds;
			if(%amount > mFloor(PlatformBricks.getCount()/1.1)) {
				%amount = mFloor(PlatformBricks.getCount()/1.1);
			}
			if(%amount < mFloor(PlatformBricks.getCount()/3)) {
				%amount = mFloor(PlatformBricks.getCount()/3);
			}
			for(%i=0;%i<%amount;%i++) {
				%brick = PlatformBricks.getObject(getRandom(0,PlatformBricks.getCount()-1));
				while(%brick.selected) {
					%brick = PlatformBricks.getObject(getRandom(0,PlatformBricks.getCount()-1));
				}
				%brick.selected = 1;

				%brick = %brick.brick;
				%brick.schedule(%i*50,playSound,brickPlantSound);

				%old_color = %brick.colorID;
				%brick.schedule(%i*50,setColor,59);
				%brick.schedule(2000+(%amount*50),fakeKillBrick,"0 0 0",2);
				%brick.schedule(2000+(%amount*50),setColor,%old_color);
			}
			$DefaultMinigame.schedule(2000+(%amount*50),playSound,"fall" @ getRandom(1,13));
			%this.specialEndSchedule = %this.schedule(8000+(%i*50)+(%amount*50)-((6-mFloor(PlatformAI.getDelayReduction()/1000))*1000),gameLoop);
		case 2:
			centerPrintAll("<font:Impact:36>\c6Look out!",5);
			%amount = mCeil(%this.rounds/4);
			for(%i=0;%i<%amount;%i++) {
				%rand = getRandom(1,4);
				schedule((1000*%i)-(PlatformAI.getDelayReduction()/15),0,fireProjectiles,%rand);
			}
			%this.specialEndSchedule = %this.schedule(2000+(1000*%i)-(PlatformAI.getDelayReduction()/20),gameLoop);
		case 3:
			%color = getRandom(0,%this.getColorAmount()-1);
			%colors[num] = getPlatformColorTypes("numbers");
			%colors[names] = getPlatformColorTypes("names");

			centerPrintAll("<font:Impact:36>\c6Memorize the colors!");
			schedule(5000,0,centerPrintAll,"<font:Impact:36>\c6Remember where <color:" @ rgbToHex(getColorIDTable(getWord(%colors[num],%color))) @ ">" @ getWord(%colors[names],%color) SPC "<color:ffffff>is!",10);
			for(%i=0;%i<PlatformBricks.getCount();%i++) {
				%row = PlatformBricks.getObject(%i);
				%brick = %row.brick;
				%brick.oldColor = %brick.colorID;
				%brick.schedule(5000,setColor,4);
				if(%row.color != %color) {
					%brick.schedule(16000,fakeKillBrick,"0 0 0",2);
				}
				%brick.schedule(16000,setColor,%brick.oldColor);
			}
			$DefaultMinigame.schedule(16000,playSound,"fall" @ getRandom(1,13));
			%this.specialEndSchedule = %this.schedule(20000,gameLoop);
		case 4:
			centerPrintAll("<font:Impact:36>\c6Jump!",5);
			for(%i=0;%i<$DefaultMinigame.numMembers;%i++) {
				%client = $DefaultMinigame.member[%i];
				if(isObject(%client.player)) {
					if(%client.player.inGame) {
						%client.player.schedule(2000,startBreakMG);
						%client.player.schedule(17000,endBreakMG);
					}
				}
			}
			%this.specialEndSchedule = %this.schedule(20000,gameLoop);
		case 5:
			centerPrintAll("<font:Impact:36>\c6Click the bricks, break the plates from under your foes!",3);
			for(%i=0;%i<$DefaultMinigame.numMembers;%i++) {
				%client = $DefaultMinigame.member[%i];
				if(isObject(%client.player)) {
					if(%client.player.inGame) {
						%client.player.canSpleefPlates = 1;
						%client.player.schedule(17000,endSpleefMG);
					}
				}
			}
			%this.specialEndSchedule = %this.schedule(20000,gameLoop);
		case 6:
			centerPrintAll("<font:Impact:36>\c6PUSHBROOMS",3);
			for(%i=0;%i<$DefaultMinigame.numMembers;%i++) {
				%client = $DefaultMinigame.member[%i];
				if(isObject(%client.player)) {
					if(%client.player.inGame) {
						%client.player.addNewItem("Push Broom");
					}
				}
			}
			$DefaultMinigame.schedule(12000,clearInGameTools);
			%this.specialEndSchedule = %this.schedule(12000,gameLoop);
	}
}
function Player::startBreakMG(%this) {
	%this.canBreakPlates = 1;
}

function Player::endBreakMG(%this) {
	%this.canBreakPlates = 0;
}

function Player::endSpleefMG(%this) {
	%this.canSpleefPlates = 0;
}

function MinigameSO::clearInGameTools(%this) {
	for(%i=0;%i<%this.numMembers;%i++) {
		%client = %this.member[%i];
		if(isObject(%client.player)) {
			if(%client.player.inGame) {
				%client.player.clearTools();
			}
		}
	}
}

function PlatformAI::stopGame(%this) {
	if(isEventPending(%this.gameSchedule)) {
		cancel(%this.gameSchedule);
		cancel(%this.specialEndSchedule);
		// make sure all players still in-game are kicked out
		messageAll('',"\c4AI: \c6Good game!");
		%this.canBet = 0;
		// blockland for some reason can't accept "if(!%this.gamesPlayed % 5) {", so
		if(%this.gamesPlayed % 5 == 0) {
			schedule(3000,0,loadLayout);
		} else {
			%this.resetSchedule = %this.schedule(3000,reset);
		}
	} else {
		messageAll('',"\c4AI: \c6There's not a game in progress. This appears to be a bug...");
	}
}

function PlatformAI::reset(%this) {
	%this.rounds = 0;
	%this.roundInitTime = -1;
	%this.inProgress = 0;
	%this.oldColorAmount = 0;
	%this.hasAwardedBonus = 0;
	%this.canBet = 0;
	%this.didBets = 0;
	%this.players = 0;
	%this.activePlayers = 0;
	%this.pot[0,amount] = 500;
	%this.pot[0,player] = "";
	%this.pot[1,amount] = 500;
	%this.pot[1,player] = "";
	cancel(%this.readySchedule);
	cancel(%this.breakSchedule);
	cancel(%this.warnSchedule);
	cancel(%this.gameSchedule);
	cancel(%this.resetSchedule);
	cancel(%this.startSchedule);

	for(%i=0;%i<ClientGroup.getCount();%i++) {
		%client = ClientGroup.getObject(%i);
		%player = %client.player;
		if(isObject(%player)) {
			if(%player.inGame) {
				%player.client.instantRespawn();
			}
		}
		%client.betContributed[amount] = 0;
		%client.betContributed[player] = "";
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
	export("$Platforms::Leaderboard*","config/server/Platforms/leaderboard.cs",0);
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
	%this.players = %this.activePlayers = %count;
	for(%i=0;%i<BrickGroup_888888.getCount();%i++) {
		%brick = BrickGroup_888888.getObject(%i);
		if(%brick.getName() $= "_spawn_teleport") {
			%brick.canTeleport = 0;
			%brick.setColor(59);
			%brick.setColorFX(0);
			%brick.setEmitter(0);
		}
	}
	%percentage = mCeil((%count/$DefaultMinigame.numMembers)*1000)/10;
	messageAll('',"\c4AI: \c6Let's begin! Approximately" SPC %percentage @ "% of the server is playing.");
	%this.gamesPlayed++;
	%this.startSchedule = %this.schedule(5000,gameLoop);
}

$Platforms::BetPercentageThreshold = 12;
//function PlatformAI::checkCurrentBets(%this,%ignore) {
//	if(!%this.canBet) {
//		return;
//	}
//	%mini = $DefaultMinigame;
//
//	for(%i=0;%i<%mini.numMembers;%i++) {
//		%client = %mini.member[%i];
//		if(%client.betContributed[player] $= "" || %client == %ignore) {
//			continue;
//		}
//
//		if(%this.pot[0,player] == %client.betContributed[player]) {
//			%losing_pot = 1;
//			%winning_pot = 0;
//		}
//		if(%this.pot[1,player] == %client.betContributed[player]) {
//			%losing_pot = 0;
//			%winning_pot = 1;
//		}
//
//		%percent = (%client.betContributed[amount]/%this.pot[%winning_pot,amount])*100;
//		%limit = mCeil(%this.pot[%winning_pot,amount]*($Platforms::BetPercentageThreshold/100));
//		if(%percent < $Platforms::BetPercentageThreshold) {
//			messageClient(%client,'',"\c6Your bet has become too small. The limit is now\c3" SPC %limit SPC "tickets.");
//			messageClient(%client,'',"\c6You have been refunded\c3" SPC %client.betContributed[amount] SPC "tickets.");
//			%client.score += %client.betContributed[amount];
//			%client.betContributed[amount] = 0;
//			%client.betContributed[player] = "";
//			%client.savePlatformsGame();
//		}
//	}
//}

function PlatformAI::doBets(%this,%winner) {
	%this.canBet = 0;
	%mini = $DefaultMinigame;

	if(%this.pot[0,player] == %winner) {
		%losing_pot = 1;
		%winning_pot = 0;
	}
	if(%this.pot[1,player] == %winner) {
		%losing_pot = 0;
		%winning_pot = 1;
	}

	for(%i=0;%i<%mini.numMembers;%i++) {
		%client = %mini.member[%i];
		if(%client.betContributed[player] !$= "") {
			if(%winner == %client.betContributed[player]) {
				%old_score = %client.score;
				//shouldn't have to worry about this anymore since we're checking everyone each time a valid bet command is done
				//%percent = (%client.betContributed[amount]/%this.pot[%winning_pot,amount])*100;
				//if(%percent < 15) {
				//	messageClient(%client,'',"\c6You did not contribute to at least 15% of the winning pot.");
				//	messageClient(%client,'',"\c6You have been refunded\c3" SPC %client.betContributed[amount] SPC "tickets.");
				//	%client.score += %client.betContributed[amount];
				//	continue;
				//}
				%percent = (%client.betContributed[amount]/%this.pot[%winning_pot,amount])*100;
				if(%percent < $Platforms::BetPercentageThreshold) {
					%client.score += %client.betContributed[amount]*2;
					messageClient(%client,'',"Your bet fell under the percentage threshold of 12%, your winnings were maxxed out at 2x.");
				} else {
					%client.score += mFloor((%client.betContributed[amount]/%this.pot[%winning_pot,amount])*(%this.pot[%losing_pot,amount]));
				}
				%client.score += %client.betContributed[amount];
				messageClient(%client,'',"\c6You have won\c3" SPC %client.score - %old_score SPC "tickets!");
				%client.savePlatformsGame();
			}
		}
	}

	%client.betContributed[amount] = 0;
	%client.betContributed[player] = "";

	%this.didBets = 1;
	%this.pot[0,amount] = 500;
	%this.pot[0,player] = "";
	%this.pot[1,amount] = 500;
	%this.pot[1,player] = "";
}

function testBetPayout(%lpot,%wpot,%contrib) {
	talk((%contrib/%wpot)*(%lpot));
}

function fireProjectiles(%which) {
	for(%i=0;%i<ProjectileBricks.getCount();%i++) {
		%row = ProjectileBricks.getObject(%i);
		%brick = %row.brick;
		if(%row.side == %which) {
			%vel = getRandom(23,25);
			switch(%which) {
				case 1:
					//%vec = -1*%vel SPC "0 0";
					%vec = "0" SPC -1*%vel SPC "0";
				case 2:
					//%vec = "0" SPC %vel SPC "0";
					%vec = %vel SPC "0 0";
				case 3:
					//%vec = %vel SPC "0 0";
					%vec = "0" SPC %vel SPC "0";
				case 4:
					//%vec = "0" SPC -1*%vel SPC "0";
					%vec = -1*%vel SPC "0 0";
			}
			%brick.spawnProjectile(%vec,GunProjectile,"0 0 0",1,"0 0 0",-1);
			%brick.spawnProjectile(%vec,GunProjectile,"0 0 0",1,"0 0 0",-1);
			%brick.spawnProjectile(%vec,GunProjectile,"0 0 0",1,"0 0 0",-1);
		}
	}
}

function PlatformAI::pregameLoop(%this) {
	cancel(%this.pregameSchedule);
	%this.pregameSchedule = %this.schedule(2000,pregameLoop);

	randomizePlatformBricks(getRandom(2,getWordCount(getPlatformColorTypes("numbers"))));
}