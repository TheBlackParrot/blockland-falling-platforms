// Taken from Clockturn's Event_setPrintText
function fxDTSbrick::setPrintText(%brick,%text,%fill,%client,%instigator)
{
	// infinite loop prevention! yaaaay
	if(isObject(%instigator))
	{
		if(%instigator.getid() == %brick.getID())
		{
			return;
		}
	} else {
		%instigator = %brick;
	}
	%db = %brick.getDatablock();
	if(!%db.hasPrint)
	{
		return;
	}
	%char = getsubstr(%text,0,1);
	if(%char $= "[")
	{
		// Check some stuff
		%pos = stripos(%text,"]");
		if(%pos != -1)
		{
			%possname = getsubstr(%text,1,%pos - 1);
			if($printNameTable[%db.printAspectRatio @ "/" @ %possname] !$= "")
			{
				%char = "[" @ %db.printAspectRatio @ "/" @ %possname;
				%text = getsubstr(%text,%pos,strlen(%text));
			}
		}
	}
	%char = resolvePrintIndex(%char);
	if(%char !$= "")
	{
		%brick.setPrint(%char);
	}
	%text = getsubstr(%text,1,strlen(%text));
	if(!%fill && strlen(%text) <= 0) // lolwut, less or equal 0?
	{
		return;
	}

	%angle = %brick.angleID;
	%vec = $Vec[%angle];
	%width = %db.brickSizeX / 2;
	%vec = vectorAdd(%brick.getWorldBoxCenter(),vectorScale(%vec,%width));
	%ray = containerRayCast(%brick.getWorldBoxCenter(),%vec,$TypeMasks::fxBrickAlwaysObjectType,%brick);
	if(isObject(%tar = getWord(%ray,0)))
	{
		%tardb = %tar.getDatablock();
		if(%tardb.hasPrint && getTrustLevel(%tar,%brick) >= 2)
		{
			%tar.setPrintText(%text,%fill,%client,%instigator);
		}
	}
}
//0123
//WSEN
$vec[0] = "1 0 0";
$vec[1] = "0 -1 0";
$vec[2] = "-1 0 0";
$vec[3] = "0 1 0";

function resolvePrintIndex(%char)
{
	if(strlen(%char) > 1)
	{
		%poss = $printNameTable[getsubstr(%char,1,strlen(%char))];
		if(%poss !$= "")
		{
			return %poss;
		} else {
			%char = getsubstr(%char,0,1);
		}
	}
	switch$(%char)
	{
	case "&": %char = "-and";
	case "'": %char = "-quote";
	case "*": %char = "-asterisk";
	case "@": %char = "-at";
	case "!": %char = "-bang";
	case "^": %char = "-caret";
	case "$": %char = "-dollar";
	case "=": %char = "-equals";
	case ">": %char = "-greater_than";
	case "<": %char = "-less_than";
	case "]": %char = "-greater_than";
	case "[": %char = "-less_than";
	case ")": %char = "-greater_than";
	case "(": %char = "-less_than";
	case "}": %char = "-greater_than";
	case "{": %char = "-less_than";
	case "-": %char = "-minus";
	case "%": %char = "-percent";
	case ".": %char = "-period";
	case "+": %char = "-plus";
	case "#": %char = "-pound";
	case "?": %char = "-qmark";
	case " ": %char = "-space";
	}
	%ind = $printNameTable["Letters/" @ %char];
	if(%ind $= "")
	{
		return $printNameTable["Letters/-space"];
	}
	return %ind;
}

function getPositionString(%num) {
	if(strLen(%num)-2 >= 0) {
		%ident = getSubStr(%num,strLen(%num)-2,2);
	} else {
		%ident = %num;
	}
	if(%ident >= 10 && %ident < 20) {
		return %num @ "th";
	}

	%ident = getSubStr(%num,strLen(%num)-1,1);
	switch(%ident) {
		case 1:
			return %num @ "st";
		case 2:
			return %num @ "nd";
		case 3:
			return %num @ "rd";
		default:
			return %num @ "th";	
	}
}

// i'm lazy
// http://forum.blockland.us/index.php?topic=271862.msg8057643#msg8057643
function findItemByName(%item)
{
	for(%i=0;%i<DatablockGroup.getCount();%i++)
	{
		%obj = DatablockGroup.getObject(%i);
		if(%obj.getClassName() $= "ItemData")
			if(strPos(%obj.uiName,%item) >= 0)
				return %obj.getName();
	}
	return -1;
}

function Player::addNewItem(%player,%item)
{
	%client = %player.client;
	if(isObject(%item))
	{
		if(%item.getClassName() !$= "ItemData") return -1;
		%item = %item.getName();
	}
	else
		%item = findItemByName(%item);
	if(!isObject(%item)) return;
	%item = nameToID(%item);
		for(%i = 0; %i < %player.getDatablock().maxTools; %i++)
		{
			%tool = %player.tool[%i];
			if(!isObject(%tool))
			{
				%player.tool[%i] = %item;
				%player.weaponCount++;
				messageClient(%client,'MsgItemPickup','',%i,%item);
				break;
			}
		}
}

function RGBToHex(%rgb) {
	%rgb = getWords(%rgb,0,2);
	for(%i=0;%i<getWordCount(%rgb);%i++) {
		%dec = mFloor(getWord(%rgb,%i)*255);
		%str = "0123456789ABCDEF";
		%hex = "";

		while(%dec != 0) {
			%hexn = %dec % 16;
			%dec = mFloor(%dec / 16);
			%hex = getSubStr(%str,%hexn,1) @ %hex;    
		}

		if(strLen(%hex) == 1)
			%hex = "0" @ %hex;
		if(!strLen(%hex))
			%hex = "00";

		%hexstr = %hexstr @ %hex;
	}

	if(%hexstr $= "") {
		%hexstr = "FF00FF";
	}
	return %hexstr;
}

function MinigameSO::centerPrintGame(%this,%msg,%duration) {
	for(%i=0;%i<%this.numMembers;%i++) {
		%client = %this.member[%i];
		if(isObject(%client.player)) {
			if(%client.player.inGame) {
				%client.centerPrint(%msg,%duration);
			}
		}
	}
}
function MinigameSO::playSoundGame(%this,%sound) {
	for(%i=0;%i<%this.numMembers;%i++) {
		%client = %this.member[%i];
		if(isObject(%client.player)) {
			if(%client.player.inGame) {
				%client.playSound(%sound);
			}
		}
	}
}

package PlatformsHatMods {
	function HatMod_getRandomHat() {
		for(%i=0;%i<getWordCount($Platforms::Achievements);%i++) {
			%id = getWord($Platforms::Achievements, %i);
			%invalid = trim(getField($Platforms::Achievement[%id], 2) SPC %invalid);
		}
		for(%i=0;%i<$Platforms::ShopItems;%i++) {
			%invalid = trim(getField($Platforms::Shop[%i], 4) SPC %invalid);
		}

		%name = HatMod_HatSet.getObject(getRandom(0, HatMod_HatSet.getCount()-1)).hatName;
		while(stripos(%invalid, %name) != -1) {
			%name = HatMod_HatSet.getObject(getRandom(0, HatMod_HatSet.getCount()-1)).hatName;
		}
		return %name;
	}
};
activatePackage(PlatformsHatMods);

function Player::hideNodesLoop(%this) {
	cancel(%this.hideNodeSched);
	%this.hideNode("ALL");
	%this.hideNodeSched = %this.schedule(1000, hideNodesLoop);
}

function mInterpolate(%var1, %var2, %weight) {
	return (1 - %weight) * %var1 + (%weight * %var2);
}
function interpolateColor(%col1, %col2, %weight, %mode) {
	for(%i=0;%i<getWordCount(%col1);%i++) {
		%num[1] = getWord(%col1, %i);
		%num[2] = getWord(%col2, %i);
		if(%mode) {
			%str = trim(%str SPC mInterpolate(%num[1], %num[2], %weight)/255);
		} else {
			%str = trim(%str SPC mFloatLength(mInterpolate(%num[1], %num[2], %weight), 0));
		}
	}
	return %str;
}