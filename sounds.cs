datablock AudioProfile(fall1)
{
	filename = "./sounds/fall1.wav";
	description = AudioClosest3d;
	preload = true;
};
$Platforms::FallCount = 1;

function initSounds() {
	if($Platforms::InitSounds) {
		return;
	}

	%pattern = "config/server/Platforms/sounds/*.wav";
	%filename = findFirstFile(%pattern);
	while(isFile(%filename)) {
		$Platforms::FallCount++;

		eval("datablock AudioProfile(fall" @ $Platforms::FallCount @ " : fall1) { filename = \"" @ %filename @ "\"; };");

		%filename = findNextFile(%pattern);
	}
	echo("Found" SPC $Platforms::FallCount SPC "fall sounds");
}
initSounds();