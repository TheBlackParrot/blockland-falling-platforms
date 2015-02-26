function gatherPlatformBricks() {
	if(isObject(PlatformBricks)) {
		for(%i=0;%i<PlatformBricks.getCount();%i++) {
			PlatformBricks.getObject(%i).delete();
		}
		PlatformBricks.initTime = getSimTime();
	} else {
		new SimSet(PlatformBricks) {
			initTime = getSimTime();
		};
	}

	for(%i=0;%i<BrickGroup_888888.getCount();%i++) {
		%brick = BrickGroup_888888.getObject(%i);
		if(%brick.getName() $= "_falling_plate") {
			%brick.isPlatform = 1;
			%obj = new ScriptObject(PlatformBrick) {
				brick = %brick;
				changed = 0;
			};
			PlatformBricks.add(%obj);
		}
	}

	talk("Gathered" SPC PlatformBricks.getCount() SPC "platform bricks.");
}

function gatherProjectileBricks() {
	if(isObject(ProjectileBricks)) {
		for(%i=0;%i<ProjectileBricks.getCount();%i++) {
			ProjectileBricks.getObject(%i).delete();
		}
		ProjectileBricks.initTime = getSimTime();
	} else {
		new SimSet(ProjectileBricks) {
			initTime = getSimTime();
		};
	}

	for(%i=0;%i<BrickGroup_888888.getCount();%i++) {
		%brick = BrickGroup_888888.getObject(%i);
		if(%brick.getName() $= "_proj_1") {
			%obj = new ScriptObject(ProjectileBrick) {
				brick = %brick;
				side = 1;
			};
		}
		if(%brick.getName() $= "_proj_2") {
			%obj = new ScriptObject(ProjectileBrick) {
				brick = %brick;
				side = 2;
			};
		}
		if(%brick.getName() $= "_proj_3") {
			%obj = new ScriptObject(ProjectileBrick) {
				brick = %brick;
				side = 3;
			};
		}
		if(%brick.getName() $= "_proj_4") {
			%obj = new ScriptObject(ProjectileBrick) {
				brick = %brick;
				side = 4;
			};
		}
		ProjectileBricks.add(%obj);
	}

	talk("Gathered" SPC ProjectileBricks.getCount() SPC "projectile bricks.");
}

function randomizePlatformBricks(%amount) { 
	%colors = getPlatformColorTypes("numbers");
	if(%amount > getWordCount(%colors)) {
		%amount = getWordCount(%colors);
	}
	%count = PlatformBricks.getCount();
	%per_brick = mFloor(%count / %amount);
	%remainder = %count - (%per_brick*%amount);

	for(%i=0;%i<%amount;%i++) {
		for(%j=0;%j<%per_brick;%j++) {
			%row = PlatformBricks.getObject(getRandom(0,%count-1));
			while(%row.changed) {
				%row = PlatformBricks.getObject(getRandom(0,%count-1));
			}
			%row.changed = 1;
			%row.brick.setColor(getWord(%colors,%i));
			%row.color = %i;
		}
	}

	for(%i=0;%i<%count;%i++) {
		%row = PlatformBricks.getObject(%i);
		if(!%row.changed) {
			%color = getRandom(0,%amount-1);
			%row.brick.setColor(getWord(%colors,%color));
			%row.color = %color;
		}
		%row.changed = 0;
	}

	//talk("Using" SPC %amount SPC "colors with" SPC %count SPC "platform bricks returns" SPC %per_brick SPC "bricks per color." SPC %remainder SPC "were left over.");
}