$Platforms::SaveDir = "config/server/Platforms/saves";

function GameConnection::loadPlatformsSave(%this) {
	%file = new FileObject();

	%file.openForRead($Platforms::SaveDir @ "/" @ sha1(%this.bl_id));
	while(!%file.isEOF()) {
		%line = %file.readLine();
		%type = getField(%line,0);
		%arg1 = getField(%line,1);
		%arg2 = getField(%line,2);
		switch$(%type) {
			case "general":
				if(%arg2 $= "") {
					return;
				}
				eval("%this." @ %arg1 @ " = \"" @ %arg2 @ "\";");
			case "date":
				%date = %arg1;
		}
	}

	%file.close();
	%file.delete();

	messageClient(%this,'',"\c6Your save from\c3" SPC %date SPC "\c6has been loaded successfully.");
}

function GameConnection::savePlatformsGame(%this) {
	%file = new FileObject();
	%file.openForWrite($Platforms::SaveDir @ "/" @ sha1(%this.bl_id));

	%file.writeLine("general" TAB "score" TAB %this.score);
	%file.writeLine("general" TAB "totalscore" TAB %this.totalscore);
	%file.writeLine("general" TAB "personalRecord" TAB %this.personalRecord);
	%file.writeLine("general" TAB "wins" TAB %this.wins);
	%file.writeLine("general" TAB "losses" TAB %this.losses);
	%file.writeLine("general" TAB "ownedItems" TAB %this.ownedItems);

	%file.writeLine("date" TAB getDateTime());

	%file.close();
	%file.delete();
}

package PlatformsSavingPackage {
	function GameConnection::autoAdminCheck(%this) {
		if(isFile($Platforms::SaveDir @ "/" @ sha1(%this.bl_id))) {
			%this.loadPlatformsSave();
		}

		return parent::autoAdminCheck(%this);
	}

	function GameConnection::spawnPlayer(%this) {
		if(isFile($Platforms::SaveDir @ "/" @ sha1(%this.bl_id))) {
			%this.loadPlatformsSave();
		}

		return parent::spawnPlayer(%this);
	}
};
activatePackage(PlatformsSavingPackage);