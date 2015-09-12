function gatherAchievements() {
	%file = new FileObject();
	%file.openForRead("Add-Ons/Gamemode_Falling_Platforms/achievements/achievements.db");

	while(!%file.isEOF()) {
		%line = %file.readLine();

		%id = getField(%line, 0);

		$Platforms::Achievement[%id] = %line;
		$Platforms::Achievements = trim($Platforms::Achievements SPC getField(%line, 0));
	}
}
gatherAchievements();

function GameConnection::awardAchievement(%this, %id) {
	if(stripos(%this.achievements, %id) != -1) {
		return;
	}

	%ach = $Platforms::Achievement[%id];
	if(%ach $= "") {
		messageAll('', "Achievement" SPC %id SPC "is not in the database.");
		return;
	}

	%this.achievements = trim(%this.achievements SPC %id);

	messageAll('', "<bitmap:base/client/ui/CI/star.png>\c3" SPC %this.name SPC "just achieved\c3" SPC getField(%ach, 1) @ "!");
	%this.play2D(rewardSound);

	%hat = getField(%ach, 2);
	if(%hat !$= "") {
		HatMod_addHat(%this, %this.bl_id, %hat, 1);
		messageClient(%this, '', "\c6You were awarded with the\c3" SPC %hat SPC "\c6hat.");
	}

	%this.savePlatformsGame();
}

package PlatformsAchievements {
	function GameConnection::autoAdminCheck(%this) {
		%date = getSubStr(getDateTime(), 0, 5);

		switch$(%date) {
			case "10/31":
				%this.schedule(200, awardAchievement, "AH0");
			case "12/25":
				%this.schedule(200, awardAchievement, "AH1");
			case "07/04":
				%this.schedule(200, awardAchievement, "AH2");
			case "01/01":
				%this.schedule(200, awardAchievement, "AH3");
		}

		return parent::autoAdminCheck(%this);
	}
};
activatePackage(PlatformsAchievements);