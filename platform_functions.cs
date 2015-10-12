function warnPlatforms(%color) {
	%colors[num] = getPlatformColorTypes("numbers");
	%colors[name] = getPlatformColorTypes("names");
	%color_brick = getWord(%colors[num],%color-1);
	if(PlatformAI.gameModifier != 3) {
		%count = PlatformBricks.getCount();
		for(%i=0;%i<%count;%i++) {
			%row = PlatformBricks.getObject(%i);
			if(%row.color == %color) {
				%row.brick.schedule(150,setColorFX,3);
				%row.brick.schedule(300,setColorFX,0);
				%row.brick.schedule(450,setColorFX,3);
				%row.brick.schedule(600,setColorFX,0);
			}
			%row.brick.setColorFX(4);
			%row.brick.setColorFX(0);
		}
		$DefaultMinigame.schedule(150,playSoundGame,brickPlantSound);
		$DefaultMinigame.schedule(450,playSoundGame,brickPlantSound);
	}
	//%str = getWord(%colors,1+%color*2);
	if(PlatformAI.gameModifier == 3) {
		%str = getWord(%colors[name],getRandom(0, getWordCount(%colors[name])-1));
	} else {
		%str = getWord(%colors[name],%color);
	}
	
	if(!PlatformAI.inverseFall) {
		//"<color:" @ rgbToHex(getColorIDTable(getWord(%colors[num],%color))) @ ">
		$DefaultMinigame.centerPrintGame("<color:" @ rgbToHex(getColorIDTable(getWord(%colors[num],%color))) @ "><font:Impact:36><just:left>" @ %str @ "<just:center>" @ %str @ "<just:right>" @ %str,5);
	} else {
		$DefaultMinigame.centerPrintGame("<color:" @ rgbToHex(getColorIDTable(getWord(%colors[num],%color))) @ "><font:Impact:36><just:left>" @ %str @ "<just:center>" @ %str @ "<just:right>" @ %str @ "<br><color:ff0000><just:left>NOT<just:center>NOT<just:right>NOT",5);
		$DefaultMinigame.playSoundGame(errorSound);
		$DefaultMinigame.schedule(300,playSoundGame,errorSound);
		$DefaultMinigame.schedule(600,playSoundGame,errorSound);
	}
	$DefaultMinigame.playSoundGame(color_notif);
}

function breakPlatforms(%color) {
	%count = PlatformBricks.getCount();
	%rand = getRandom(0, 3);
	for(%i=0;%i<%count;%i++) {
		%row = PlatformBricks.getObject(%i);
		%row.brick.setColorFX(0);

		if(!PlatformAI.inverseFall) {
			if(%row.color != %color) {
				if(!%rand) {
					%row.brick.fakeKillBrick(getRandom(-50, 50) SPC getRandom(-50, 50) SPC getRandom(-50, 50), 6-mFloor(PlatformAI.getDelayReduction()/1000));
				} else {
					%row.brick.fakeKillBrick("0 0 0",6-mFloor(PlatformAI.getDelayReduction()/1000));
				}
			}
		} else {
			if(%row.color == %color) {
				if(!%rand) {
					%row.brick.fakeKillBrick(getRandom(-50, 50) SPC getRandom(-50, 50) SPC getRandom(-50, 50), 6-mFloor(PlatformAI.getDelayReduction()/1000));
				} else {
					%row.brick.fakeKillBrick("0 0 0",6-mFloor(PlatformAI.getDelayReduction()/1000));
				}
			}
		}
	}
	$DefaultMinigame.playSoundGame("fall" @ getRandom(1,30));
}

function warnPracticePlatforms(%color) {
	%colors[num] = getPlatformColorTypes("numbers");
	%color_brick = getWord(%colors[num],%color-1);
	%count = PlatformPracticeBricks.getCount();
	for(%i=0;%i<%count;%i++) {
		%row = PlatformPracticeBricks.getObject(%i);
		if(%row.color == %color) {
			%row.brick.schedule(150,setColorFX,3);
			%row.brick.schedule(300,setColorFX,0);
			%row.brick.schedule(450,setColorFX,3);
			%row.brick.schedule(600,setColorFX,0);
		}
		%row.brick.setColorFX(4);
		%row.brick.setColorFX(0);
	}
}
function breakPracticePlatforms(%color) {
	%count = PlatformPracticeBricks.getCount();
	for(%i=0;%i<%count;%i++) {
		%row = PlatformPracticeBricks.getObject(%i);
		%row.brick.setColorFX(0);
		if(%row.color != %color) {
			%row.brick.disappear(6-mFloor(PlatformAI.getDelayReduction(17)/1000));
		}
	}
}