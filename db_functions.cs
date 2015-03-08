if(!$Mining::HasInit) {
	if(isObject(PlatformPracticeBricks)) {
		PlatformPracticeBricks.clear();
	} else {
		new SimSet(PlatformPracticeBricks) {
			initTime = getSimTime();
		};
	}
	if(isObject(RocketBricks)) {
		RocketBricks.clear();
	} else {
		new SimSet(RocketBricks) {
			initTime = getSimTime();
		};
	}
}

function gatherPlatformBricks() {
	if(isObject(PlatformBricks)) {
		PlatformBricks.clear();
	} else {
		new SimSet(PlatformBricks) {
			initTime = getSimTime();
		};
	}

	if(!BrickGroup_Platforms.getCount()) {
		return;
	}

	for(%i=0;%i<BrickGroup_Platforms.getCount();%i++) {
		%brick = BrickGroup_Platforms.getObject(%i);
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
		ProjectileBricks.clear();
	} else {
		new SimSet(ProjectileBricks) {
			initTime = getSimTime();
		};
	}

	for(%i=0;%i<BrickGroup_Platforms.getCount();%i++) {
		%brick = BrickGroup_Platforms.getObject(%i);
		if(isObject(%brick)) {
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
		} else {
			echo("...why exactly does this else statement fix this?");
		}
		if(isObject(%obj)) {
			ProjectileBricks.add(%obj);
		}
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

function randomizePracticePlatformBricks(%amount) { 
	%colors = getPlatformColorTypes("numbers");
	if(%amount > getWordCount(%colors)) {
		%amount = getWordCount(%colors);
	}
	%count = PlatformPracticeBricks.getCount();
	%per_brick = mFloor(%count / %amount);
	%remainder = %count - (%per_brick*%amount);

	for(%i=0;%i<%amount;%i++) {
		for(%j=0;%j<%per_brick;%j++) {
			%row = PlatformPracticeBricks.getObject(getRandom(0,%count-1));
			while(%row.changed) {
				%row = PlatformPracticeBricks.getObject(getRandom(0,%count-1));
			}
			%row.changed = 1;
			%row.brick.setColor(getWord(%colors,%i));
			%row.color = %i;
		}
	}

	for(%i=0;%i<%count;%i++) {
		%row = PlatformPracticeBricks.getObject(%i);
		if(!%row.changed) {
			%color = getRandom(0,%amount-1);
			%row.brick.setColor(getWord(%colors,%color));
			%row.color = %color;
		}
		%row.changed = 0;
	}

	//talk("Using" SPC %amount SPC "colors with" SPC %count SPC "platform bricks returns" SPC %per_brick SPC "bricks per color." SPC %remainder SPC "were left over.");
}

function gatherLayouts() {
	if(isObject(PlatformLayouts)) {
		PlatformLayouts.clear();
	} else {
		new SimSet(PlatformLayouts) {
			initTime = getSimTime();
			class = PlatformLayoutsSet;
		};
	}

	%path = $Platforms::LayoutDir;
	for(%file = findFirstFile(%path @ "*.bls");%file !$= "";%file = findNextFile(%path @ "*.bls")) {
		if(fileExt(%file) !$= ".bls") {
			continue;
		}
		%row = new ScriptObject(PlatformLayout) {
			file = %file;
			plays = 0;
			lastLoaded = "";
		};
		PlatformLayouts.add(%row);
	}
}

function gatherTipBotLines() {
	%file = new FileObject();
	%filename = "config/server/Platforms/tips.txt";
	if(!isFile(%filename)) {
		%filename = "Add-Ons/Gamemode_Falling_Platforms/tips.txt";
	}
	%file.openForRead(%filename);

	%line_count = 0;
	while(!%file.isEOF()) {
		%line = %file.readLine();
		if(getSubStr(%line,0,2) $= "//") {
			continue;
		}

		$Platforms::Tip[%line_count] = %line;
		%line_count++;
	}

	%file.close();
	%file.delete();
}