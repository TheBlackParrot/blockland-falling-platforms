function warnPlatforms(%color) {
	%colors = "0 red 3 blue 1 yellow 2 green 8 black 4 white 17 purple 14 orange";
	%count = PlatformBricks.getCount();
	for(%i=0;%i<%count;%i++) {
		%row = PlatformBricks.getObject(%i);
		if(%row.color == %color) {
			%row.brick.setColorFX(3);
		}
	}
	%str = getWord(%colors,1+%color*2);
	centerPrintAll("<color:" @ rgbToHex(getColorIDTable(getWord(%colors,%color*2))) @ "><font:Impact:36><just:left>" @ %str @ "<just:center>" @ %str @ "<just:right>" @ %str,5);
	serverPlay2D(color_notif);
}

function breakPlatforms(%color) {
	%count = PlatformBricks.getCount();
	for(%i=0;%i<%count;%i++) {
		%row = PlatformBricks.getObject(%i);
		%row.brick.setColorFX(0);
		if(%row.color != %color) {
			%row.brick.fakeKillBrick(getRandom(-20,20) SPC getRandom(-20,20) SPC getRandom(-20,20),5-mFloor(PlatformAI.getDelayReduction()/1000));
		}
	}
	serverPlay2D("fall" @ getRandom(1,13));
}