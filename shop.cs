//HatMod_addHat(%client,%bl_id,%hat,%amount)
//unneeded arguments, hooray
function initHatDB() {
	%file = new FileObject();
	%filename = "config/server/Platforms/shop/hats.db";
	if(!isFile(%filename)) {
		%filename = "Add-Ons/Gamemode_Falling_Platforms/shop/hats.db";
	}
	%file.openForRead(%filename);

	while(!%file.isEOF()) {
		%line = %file.readLine();
		if(getSubStr(%line,0,2) $= "//") {
			continue;
		}
		// using a starting index of 1 for a reason
		%line_count++;

		$Platforms::Shop::Hat[%line_count,hat] = getField(%line,0);
		$Platforms::Shop::Hat[%line_count,cost] = getField(%line,1);
	}

	%file.close();
	%file.delete();

	talk("Gathered" SPC %line_count SPC "hats");
}
initHatDB();

function serverCmdBuy(%this,%item) {
	if(%item < 1 || %item > 56) {
		return;
	}
	// add an if check here if this is ever expanded beyond hats
	%hat[name] = $Platforms::Shop::Hat[%item,hat];
	%hat[cost] = $Platforms::Shop::Hat[%item,cost];
	if(%this.hasHat(%hat[name])) {
		messageClient(%this,'',"\c6You already have the\c3" SPC %hat[name] SPC "hat!");
		return;
	}
	if(%this.score < %hat[cost]) {
		messageClient(%this,'',"\c6You do not have\c3" SPC %hat[cost] SPC "tickets \c6to buy the\c3" SPC %hat[name] SPC "hat.");
		return;
	} else {
		%this.score -= %hat[cost];
		%this.savePlatformsGame();
		messageClient(%this,'',"\c6You just bought the\c3" SPC %hat[name] SPC "\c3hat \c6for\c3" SPC %hat[cost] SPC "tickets!");
		HatMod_addHat(%this,%this.bl_id,%hat[name],1);
	}
}