if(!$Platforms::HasInit) {
	new SimGroup(BrickGroup_Platforms : BrickGroup_888888);
	mainBrickGroup.add(BrickGroup_Platforms);
}

function loadLayout(%which) {
	// blockland pls
	// let me set commands to simsets
	// pls
	%this = PlatformLayouts;

	PlatformAI.loadingLayout = 1;
	%group = BrickGroup_Platforms;
	%group.deleteAll();
	messageAll('',"\c4AI: \c6Loading a new layout...");

	if(!%which) {
		if(%this.currentLayout $= "") {
			%this.currentLayout = 0;
		} else {
			%this.currentLayout++;
		}
		if(%this.currentLayout >= %this.getCount()) {
			%this.currentLayout = 0;
		}
		%which = %this.currentLayout;
	}
	if(BrickGroup_Platforms.getGroup() $= "") {
		mainBrickGroup.add(BrickGroup_Platforms);
	}
	$LoadingBricks_BrickGroup = BrickGroup_Platforms;
	$LoadingBricks_Client = -1;
	$LoadingBricks_ColorMethod = 3; //should always be 3
	$LoadingBricks_FileName = %this.getObject(%which).file;
	$LoadingBricks_Silent = true;
	$LoadingBricks_StartTime = getSimTime();
	ServerLoadSaveFile_Start(%this.getObject(%which).file);
}

package FallingPlatformsLayoutPackage {
	function fxDTSBrick::onLoadPlant(%this) {
		parent::onLoadPlant(%this);

		if(PlatformAI.loadingLayout) {
			BrickGroup_Platforms.schedule(1,add,%this);
		}
	}

	function ServerLoadSaveFile_End() {
		parent::ServerLoadSaveFile_End();

		if(!$Platforms::HasInit) {
			gatherLayouts();
			loadLayout(getRandom(0,PlatformLayouts.getCount()-1));
			if(isFile("config/server/Platforms/leaderboard.cs")) {
				exec("config/server/Platforms/leaderboard.cs");
			}
			$Platforms::HasInit = 1;
			// going to lag initially, but the practice room is forever a square so we shouldn't have to worry here.
			for(%i=0;%i<BrickGroup_888888.getCount();%i++) {
				%brick = BrickGroup_888888.getObject(%i);
				if(%brick.getName() $= "_practice_plate") {
					%obj = new ScriptObject(PlatformBrick) {
						brick = %brick;
						changed = 0;
					};
					PlatformPracticeBricks.add(%obj);
				}
			}
			return;
		}
		if(PlatformAI.loadingLayout) {
			PlatformAI.loadingLayout = 0;
			PlatformAI.schedule(200,reset);
		}

		echo("GETTING PLATFORM BRICKS");
		gatherPlatformBricks();
		echo("GETTING PROJECTILE BRICKS");
		schedule(500,0,gatherProjectileBricks);
	}
};
activatePackage(FallingPlatformsLayoutPackage);