function clearLeaderboardBricks() {
	for(%i=1;%i<=26;%i++) {
		%brick = "_leaderboard_" @ %i;
		%brick.setPrintText("",1);
	}
}
function addToLeaderboard(%client) {
	for(%i=1;%i<=26;%i++) {
		if(%client.name $= $Platforms::Leaderboard[%i,name]) {
			$Platforms::Leaderboard[%i,amount] = %client.totalscore || 0;
			%player_found = 1;
		}
		if($Platforms::Leaderboard[%i,name] $= "") {
			if(!%player_found) {
				$Platforms::Leaderboard[%i,name] = %client.name;
				$Platforms::Leaderboard[%i,amount] = %client.totalscore || 0;
				%spots = %i;
			} else {
				%spots = %i-1;
			}
			break;
		}
	}
	if(!%spots) {
		%spots = 27;
		if(!%player_found) {
			$Platforms::Leaderboard[27,name] = %client.name;
			$Platforms::Leaderboard[27,amount] = %client.totalscore;
		}
	}
	for(%i=1;%i<=%spots;%i++) {
		for(%j=1;%j<=%spots;%j++) {
			if(!%selected[%j]) {
				if(%highest[%i,amount] <= $Platforms::Leaderboard[%j,amount]) {
					%highest[%i,amount] = $Platforms::Leaderboard[%j,amount];
					%highest[%i,name] = $Platforms::Leaderboard[%j,name];
					%selected_spot = %j;
				}
			}
		}
		%selected[%selected_spot] = 1;
	}
	for(%i=1;%i<=%spots;%i++) {
		$Platforms::Leaderboard[%i,amount] = %highest[%i,amount];
		$Platforms::Leaderboard[%i,name] = %highest[%i,name];
		if(%i<27) {
			%spaces = "..........................";
			%brick = "_leaderboard_" @ %i;

			%potential_target = findclientbyname($Platforms::Leaderboard[%i,name]);
			if(isObject(%potential_target)) {
				%potential_target.clanPosition = "\c7[\c2" @ getPositionString(%i)  @ "\c7]";
				%potential_target.clanPrefix = %potential_target.clanPosition SPC %potential_target.original_prefix;
			}

			if(strLen(%i) < 2) {
				%pos = "0" @ %i;
			} else {
				%pos = %i;
			}
			%name = getSubStr($Platforms::Leaderboard[%i,name],0,26);
			%score = getSubStr(%spaces,0,strLen(%spaces)-strLen(%name)) @ getSubStr("..........",0,10-strLen($Platforms::Leaderboard[%i,amount])) @ $Platforms::Leaderboard[%i,amount];
			%brick.setPrintText(%pos SPC %name @ %score,1);
		} else {
			%potential_target = findclientbyname($Platforms::Leaderboard[27,name]);
			if(isObject(%potential_target)) {
				%potential_target.clanPrefix = %potential_target.original_prefix;
			}
		}
	}
	$Platforms::Leaderboard[27,name] = "";
	$Platforms::Leaderboard[27,amount] = 0;
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
		return parent::onDeath(%this,%obj,%killer,%type,%area);
	}
};
activatePackage(PlatformsLeaderboardPackage);