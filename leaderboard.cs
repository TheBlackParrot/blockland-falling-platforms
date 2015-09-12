function clearLeaderboardBricks() {
	for(%i=1;%i<=26;%i++) {
		%brick = "_leaderboard_" @ %i;
		%brick.setPrintText("",1);
	}
}

function addToLeaderboard(%client) {
	// $Platforms::Leaderboard[pos,[name,amount]]
	if(!isObject(PlatformsLeaderboard)) {
		// awaiting the day to see http://www.garagegames.com/community/resources/view/4711 added
		new GuiTextListCtrl(PlatformsLeaderboard);
	}

	%list = PlatformsLeaderboard;
	%data = %list.getRowTextByID(%client.bl_id);
	if(%data $= "") {
		%list.addRow(%client.bl_id, %client.name @ "\t" @ (%client.totalscore || 0) @ "\t" @ %client.bl_id);
	} else {
		%list.setRowByID(%client.bl_id, %client.name @ "\t" @ (%client.totalscore || 0) @ "\t" @ %client.bl_id);
	}

	%list.sortNumerical(1, 0);
}

function PlatformsLeaderboard::saveLeaderboard(%this) {
	%filename = "config/server/Platforms/leaderboard.db";

	%file = new FileObject();
	%file.openForWrite(%filename);

	for(%i=0;%i<%this.rowCount();%i++) {
		%file.writeLine(%this.getRowText(%i));
	}

	%file.close();
	%file.delete();
}

function loadLeaderboard(%this) {
	if(isObject(PlatformsLeaderboard)) {
		return;
	} else {
		new GuiTextListCtrl(PlatformsLeaderboard);
	}

	%filename = "config/server/Platforms/leaderboard.db";

	%file = new FileObject();
	%file.openForRead(%filename);

	%list = PlatformsLeaderboard;

	while(!%file.isEOF()) {
		%line = %file.readLine();

		%id = getField(%line, 2);
		%score = getField(%line, 1);
		if(%id $= "" || %score $= "") {
			continue;
		}

		%list.addRow(%id, %line);
	}

	%file.close();
	%file.delete();

	%list.sortNumerical(1, 0);
}

package PlatformsLeaderboardPackage {
	function GameConnection::onDeath(%this,%obj,%killer,%type,%area) {
		if(!isObject(%this.player)) {
			return parent::onDeath(%this,%obj,%killer,%type,%area);
		}
		if(!%this.player.inGame) {
			return parent::onDeath(%this,%obj,%killer,%type,%area);
		}
		addToLeaderboard(%this);
		%this.clanPrefix = "\c7[\c5" @ getPositionString(PlatformsLeaderboard.getRowNumByID(%this.bl_id)+1) @ "\c7]" SPC %this.original_prefix;
		return parent::onDeath(%this,%obj,%killer,%type,%area);
	}
};
activatePackage(PlatformsLeaderboardPackage);