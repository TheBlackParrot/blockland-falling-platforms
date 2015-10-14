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
	addToLeaderboard(%this);
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
	%file.writeLine("general" TAB "achievements" TAB %this.achievements);

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


function clearTicketSaves() {
	talk("WIPING TICKETS.");
	%pattern = $Platforms::SaveDir @ "/*";
	%filename = findFirstFile(%pattern);

	%file_old = new FileObject();
	%file_new = new FileObject();

	while(isFile(%filename)) {
		%i = 0;
		%file_old.openForRead(%filename);

		while(!%file_old.isEOF()) {
			%line[%i] = %file_old.readLine();
			if(getField(%line[%i], 1) $= "score") {
				%line[%i] = "general" TAB "score" TAB 0;
			}
			%i++;
		}
		%file_old.close();

		%file_new.openForWrite(%filename);
		for(%j=0;%j<%i;%j++) {
			%file_new.writeLine(%line[%j]);
		}
		%file_new.close();

		%filename = findNextFile(%pattern);
	}

	%file_old.delete();
	%file_new.delete();
	
	for(%i=0;%i<ClientGroup.getCount();%i++) {
		ClientGroup.getObject(%i).score = 0;
	}
}

function reimbursePlayers() {
	talk("REIMBURSING PLAYERS.");
	%pattern = $Platforms::SaveDir @ "/*";
	%filename = findFirstFile(%pattern);

	%file_old = new FileObject();
	%file_new = new FileObject();

	while(isFile(%filename)) {
		%i = 0;
		%total = 0;
		%file_old.openForRead(%filename);

		while(!%file_old.isEOF()) {
			%line[%i] = %file_old.readLine();
			if(getField(%line[%i], 1) $= "wins") {
				%total += (getField(%line[%i], 2) ? getField(%line[%i], 2) : 0) * 750;
			}
			if(getField(%line[%i], 1) $= "losses") {
				%total += (getField(%line[%i], 2) ? getField(%line[%i], 2) : 0) * 250;
			}
			for(%j=0;%j<%i;%j++) {
				%file_new.writeLine(%line[%j]);
			}
			%i++;
		}
		for(%j=0;%j<%i;%j++) {
			if(getField(%line[%j], 1) $= "score") {
				%line[%j] = "general" TAB "score" TAB %total;
			}
		}
		%file_old.close();

		%file_new.openForWrite(%filename);
		for(%j=0;%j<%i;%j++) {
			%file_new.writeLine(%line[%j]);
		}
		%file_new.close();

		%filename = findNextFile(%pattern);
	}

	%file_old.delete();
	%file_new.delete();
	
	for(%i=0;%i<ClientGroup.getCount();%i++) {
		ClientGroup.getObject(%i).loadPlatformsSave();
	}
}