function serverCmdHelp(%this) {
	messageClient(%this,'',"\c3/changeMusic \c5[num] \c6-- Changes the music on the server, leave it blank to see a list. \c7[250 points]");
}

function serverCmdChangeMusic(%this,%which) {
	if(%which $= "" || %which <= 0 || %which > $Platforms::MusicDataCount) {
		for(%i=0;%i<$Platforms::MusicDataCount;%i++) {
			%music_obj = "musicData_FP" @ %i;
			messageClient(%this,'',"\c3" @ %i+1 @ ".\c6" SPC %music_obj.uiName);
		}
		return;
	}
	if(%this.score < 250) {
		messageClient(%this,'',"\c6You need at least 250 points to change the music.");
		return;
	}
	if(getSimTime() - $Platforms::LastMusicChange < 120000) {
		messageClient(%this,'',"\c6The music can only be changed once every 2 minutes. Please wait another\c3" SPC mFloor((getSimTime() - $Platforms::LastMusicChange)/1000) SPC "second(s).");
		return;
	}

	// index starts at 0, we show the client starting at 1
	%which -= 1;

	%this.score -= 250;
	%music_obj = "musicData_FP" @ %which;
	$Platforms::LastMusicChange = getSimTime();
	messageAll('',"\c3" @ %this.name SPC "\c6changed the music to\c3" SPC %music_obj.uiName);

	// i would just set the profile and update it, but that's not working
	// i bet this is why the default music bricks have a new AudioEmitter everytime it's changed
	if(isObject($Platforms::Music)) {
		$Platforms::Music.delete();
	}
	$Platforms::Music = new AudioEmitter() {
		is3D = 0;
		profile = "musicData_FP" @ %which;
		referenceDistance = 999999;
		maxDistance = 999999;
		volume = 0.6;
	};
}