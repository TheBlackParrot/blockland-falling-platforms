function GameConnection::itemInfo(%this, %item) {
	if($Platforms::Shop[%item] $= "") {
		messageClient(%this, '', %item SPC "doesn't exist in the shop database. :(");
		return;
	}

	%name = getField($Platforms::Shop[%item], 1);
	%cost = getField($Platforms::Shop[%item], 2);
	// %id = getField($Platforms::Shop[%item], 3);

	%playerCost = %cost - %this.score;
	if(%playerCost > 0) {
		messageClient(%this, '', "\c3The" SPC %name SPC "\c6costs\c5" SPC %cost SPC "tickets. \c6You need\c5" SPC %cost - %this.score SPC "more tickets \c6to purchase this item.");
	} else {
		messageClient(%this, '', "\c3The" SPC %name SPC "\c6costs\c5" SPC %cost SPC "tickets. \c6You have enough tickets to purchase this item.");
	}
}

function GameConnection::purchaseItem(%this, %item) {
	if($Platforms::Shop[%item] $= "") {
		messageClient(%this, '', %item SPC "doesn't exist in the shop database. :(");
		return;
	}

	%name = getField($Platforms::Shop[%item], 1);
	%cost = getField($Platforms::Shop[%item], 2);
	%id = getField($Platforms::Shop[%item], 3);

	if(stripos(%this.ownedItems, %id) != -1) {
		messageClient(%this, '', "You already own the\c3" SPC %name @ "\c0!");
		return;
	}

	if(%this.score < %cost) {
		messageClient(%this, '', "You do not have enough tickets to purchase the\c3" SPC %name @ "\c0! \c6You need\c5" SPC %cost - %this.score SPC "more tickets.");
		return;
	}

	%this.score -= %cost;
	%this.ownedItems = %this.ownedItems SPC %id;

	messageClient(%this, '', "\c6You just bought the\c3" SPC %name SPC "\c6for\c5" SPC %cost SPC "tickets!");
	%this.savePlatformsGame();

	if(getSubStr(%id, 0, 1) $= "H") {
		%pname = getField($Platforms::Shop[%item], 4);
		HatMod_addHat(%this, %this.bl_id, %pname, 1);
	}
}

function initiateShopEvents() {
	%file = new FileObject();
	%file.openForRead("Add-Ons/Gamemode_Falling_Platforms/shop/shop.db");

	%i = 0;
	while(!%file.isEOF()) {
		%line = %file.readLine();

		$Platforms::Shop[%i] = %line;
		
		%str = %str SPC getField(%line, 0) SPC %i;
		%i++;
	}
	$Platforms::ShopItems = %i+1;

	%str = trim(%str);
	echo(%str);

	registerOutputEvent(GameConnection, "itemInfo", "list" SPC %str, 1);
	registerOutputEvent(GameConnection, "purchaseItem", "list" SPC %str, 1);
}
initiateShopEvents();


function serverCmdSetSlotBet(%client, %amount) {
	%amount = mFloor(%amount);
	
	if(%amount $= "" || %amount < 50) {
		%amount = 50;
		messageClient(%client, '', "\c0There is a minimum bet of 50 tickets.");
	}
	if(%amount >= 1000000) {
		%amount = 999999;
		messageClient(%client, '', "\c0There is a maximum bet of 999,999 tickets.");
	}

	%client.slotBetAmount = %amount;
	messageClient(%client, '', "\c6You are betting\c3" SPC %amount SPC "tickets on slots.");
}

function startSlots(%client) {
	if($Platforms::SlotsGoing) {
		return;
	}

	$Platforms::SlotsGoing = 1;

	%amount = %client.slotBetAmount;
	if(!%amount) {
		%client.slotBetAmount = %amount = 50;
	}

	_jackpot_bet.setPrintText(getSubStr("      ", 0, 6-strLen(%amount)) @ %amount, 1);
	_jackpot_spinner.setPrintText(%client.name, 1);

	%client.score -= %client.slotBetAmount;
	$Platforms::TempSlotsAmount = %client.slotBetAmount;

	$Platforms::SlotsJackpot += %client.slotBetAmount;
	
	%str = mFloor($Platforms::SlotsJackpot);
	_jackpot_amount.setPrintText(getSubStr("         ", 0, 9-strLen(%str)) @ %str, 1);

	slotDig(1, %client);
}

function slotDig(%digit, %client) {
	cancel($Platforms::SlotSched);

	if(!$Platforms::SlotsGoing) {
		_jackpot_spin.disappear(0);
		return;
	}

	if(%digit > 3) {
		endSlots(%client);
		return;
	}

	%brick = "_jackpot_d" @ %digit;
	%brick.setPrint(getRandom(22, 35));

	if(getRandom(0, 24)) {
		%brick.playSound(brickPlantSound);
		$Platforms::SlotSched = schedule(50+getRandom(-30, 10), 0, slotDig, %digit, %client);
	} else {
		%brick.playSound(brickRotateSound);
		$Platforms::SlotSched = schedule(50+getRandom(-30, 10), 0, slotDig, %digit+1, %client);
	}

	//$Platforms::SlotSched
}

function isSlotDigitsEqual() {
	if(_jackpot_d1.printID == _jackpot_d2.printID && _jackpot_d1.printID == _jackpot_d3.printID) { return 2; }
	if(_jackpot_d1.printID == _jackpot_d2.printID || _jackpot_d1.printID == _jackpot_d3.printID || _jackpot_d2.printID == _jackpot_d3.printID) { return 1; }
	return 0;
}

function endSlots(%client) {
	$Platforms::SlotsGoing = 0;

	%eq = isSlotDigitsEqual();
	switch(%eq) {
		case 2:
			messageAll('MsgAdminForce', "\c3" @ %client.name SPC "\c5JUST WON THE JACKPOT! \c3(" @ mFloor($Platforms::SlotsJackpot) SPC "tickets)");
			%client.awardAchievement("A0A");
			
			%client.score += $Platforms::SlotsJackpot;
			$Platforms::SlotsJackpot = 0;

			$DefaultMinigame.playSound(Beep_Siren_Sound);
			$DefaultMinigame.schedule(1500, playSound, Beep_Siren_Sound);
			$DefaultMinigame.schedule(3000, playSound, Beep_Siren_Sound);

			_jackpot_spin.disappear(5);

		case 1:
			%client.score += $Platforms::TempSlotsAmount * 2;
			messageClient(%client, '', "\c6You won\c3" SPC mFloor($Platforms::TempSlotsAmount * 2) SPC "tickets!");
			%client.playSound(Beep_Popup_Sound);

			_jackpot_spin.disappear(0);

		case 0:
			messageClient(%client, '', "\c6You didn't win anything, sorry.");
			%client.playSound(Beep_EKG_Sound);

			_jackpot_spin.disappear(0);
	}

	%client.savePlatformsGame();
}

package PlatformSlotPackage {
	function Player::ActivateStuff(%this) {
		%eye = vectorScale(%this.getEyeVector(), 10);
		%pos = %this.getEyePoint();
		%mask = $TypeMasks::FxBrickObjectType;
		%hit = firstWord(containerRaycast(%pos, vectorAdd(%pos, %eye), %mask, %this));
			
		if(!isObject(%hit)) {
			return parent::ActivateStuff(%this);
		}
			
		if(%hit.getClassName() $= "fxDTSBrick") {
			if(%hit == _jackpot_spin.getID()) {
				if(%this.client.score < (%this.client.slotBetAmount || 50)) {
					messageClient(%this.client, '', "You do not have enough tickets to spin.");
					return parent::ActivateStuff(%this);
				}
				%this.addVelocity("0 30 0");
				%hit.disappear(-1);
				startSlots(%this.client);
			}
		}

		return parent::ActivateStuff(%this);
	}
};
activatePackage(PlatformSlotPackage);